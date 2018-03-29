using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedge
{
    namespace Tools
    {
        public static class DrawTools
        {
            static public float thickness = 0.0005f;
            static public float gap = 0.03f;
            static public float dashLength = 0.02f;
            static Material lineMaterial = new Material(Shader.Find("Unlit/Color"));
            static public Color LineColor
            {
                get { return lineMaterial.color; }
                set
                {
                    lineMaterial.color = value;
                    lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            static public void SetProperties(Color color)
            {
                //Надо вытащить куда-то
                var shader = Shader.Find("Unlit/Color");
                lineMaterial = new Material(shader);
                lineMaterial.color = color;
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                //
            }

            static public void TestDraw(Vector2 begin, Vector2 end)
            {
                lineMaterial.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Color(Color.red);
                GL.Vertex(begin);
                GL.Color(Color.red);
                GL.Vertex(end);
                GL.End();
            }

            static public void DrawLine(Vector2 begin, Vector2 end, float orthographic_size = 1, bool dashed = false, float ratio = 1)
            {
                float current_gap = gap; //* orthographic_size;
                float current_dashLength = dashLength; //* orthographic_size;
                float current_thickness =  thickness; //*orthographic_size;

                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.LoadOrtho();            

                GL.Begin(GL.QUADS);
                Vector2 startPoint;
                Vector2 endPoint;

                //var diffx = Mathf.Abs((begin.x - end.x));
                //var diffy = Mathf.Abs((begin.y - end.y));

                /*if (diffx > diffy)
                {
                    if (begin.x <= end.x)
                    {
                        startPoint = begin;
                        endPoint = end;
                    }
                    else
                    {
                        startPoint = end;
                        endPoint = begin;
                    }
                }
                else
                {
                    if (begin.y <= end.y)
                    {
                        startPoint = begin;
                        endPoint = end;
                    }
                    else
                    {
                        startPoint = end;
                        endPoint = begin;
                    }
                }*/

                startPoint = begin;
                endPoint = end;

                float angleToX = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
                float angleToY = Mathf.Atan2(endPoint.x - startPoint.x, -(endPoint.y - startPoint.y));

               

                float cosToXaxis = Mathf.Cos(angleToX);
                float cosToYaxis = Mathf.Cos(angleToY );

                float sinToXaxis = Mathf.Sin(angleToX);
                float sinToYaxis = Mathf.Sin(angleToY);

                float temp = Mathf.Abs(cosToXaxis);
                if (temp >= 0 && temp < 1e-4) cosToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToXaxis = cosToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(cosToYaxis);
                if (temp >= 0 && temp < 1e-4) cosToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToYaxis = cosToYaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToXaxis);
                if (temp >= 0 && temp < 1e-4) sinToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToXaxis = sinToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToYaxis);
                if (temp >= 0 && temp < 1e-4) sinToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToYaxis = sinToYaxis > 0 ? 1 : -1;

                var distance = Vector2.Distance(startPoint, endPoint);

                Vector3 p1, p2, p3, p4;
                if (dashed)
                {
                    
                    //float stepLenght = gap * Mathf.Sqrt(cosToXaxis* cosToXaxis + ratio* sinToXaxis* sinToXaxis);
                    float totalDashes = distance / current_gap;
                    float stepByX = current_gap * cosToXaxis;
                    //var stepByY = current_gap * sinToXaxis;
                    float stepByY = ratio * gap * sinToXaxis;
                    Vector2 step = new Vector2(stepByX, stepByY);
                    Vector2 currentDashStartPoint = new Vector2(startPoint.x + current_dashLength/2 * cosToXaxis, startPoint.y + current_dashLength/2 * sinToXaxis) ;
                    for (int n =0; n<=totalDashes; n++)
                    {

                        p1.x = currentDashStartPoint.x - current_thickness * cosToYaxis;
                        p1.y = currentDashStartPoint.y - current_thickness * sinToYaxis;

                        p2.x = currentDashStartPoint.x + current_thickness * cosToYaxis;
                        p2.y = currentDashStartPoint.y + current_thickness * sinToYaxis;

                        p3.x = p2.x + current_dashLength * cosToXaxis;
                        p3.y = p2.y + ratio*dashLength * sinToXaxis;
                        //p3.y = p2.y + current_dashLength * sinToXaxis;

                        p4.x = p1.x + current_dashLength * cosToXaxis;
                        p4.y = p1.y + ratio * dashLength * sinToXaxis;
                        //p4.y = p1.y + current_dashLength * sinToXaxis;

                        currentDashStartPoint += step;
                        GL.Vertex3(p1.x, p1.y, 1);
                        GL.Vertex3(p2.x, p2.y, 1);
                        GL.Vertex3(p3.x, p3.y, 1);
                        GL.Vertex3(p4.x, p4.y, 1);
                    }
                    
                }
                else
                {
                    p1.x = startPoint.x - current_thickness * cosToYaxis;
                    p1.y = startPoint.y - current_thickness * sinToYaxis;

                    p2.x = startPoint.x + current_thickness * cosToYaxis;
                    p2.y = startPoint.y + current_thickness * sinToYaxis;

                    p3.x = p2.x + distance * cosToXaxis;
                    p3.y = p2.y +  distance * sinToXaxis ;

                    p4.x = p1.x + distance * cosToXaxis;
                    p4.y = p1.y +  distance * sinToXaxis;

                    GL.Vertex3(p1.x, p1.y, 1);
                    GL.Vertex3(p2.x, p2.y, 1);
                    GL.Vertex3(p3.x, p3.y, 1);
                    GL.Vertex3(p4.x, p4.y, 1);
                }



                GL.End();
                GL.PopMatrix();
            }
        }
    }

}

