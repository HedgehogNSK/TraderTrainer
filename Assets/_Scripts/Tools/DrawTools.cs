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
            static public float thickness = 0.001f;
            static public float gap = 0.08f;
            static public float dashLength = 0.05f;
            static Material lineMaterial = new Material(Shader.Find("Unlit/Color"));
            static public Color color;
            static Vector3 zeroVector = Vector3.zero;
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

            static public void DrawLine(Vector2 begin, Vector2 end, float orthographic_size = 1, bool dashed = false)
            {
                float current_gap = gap * orthographic_size;
                float current_dashLength = dashLength * orthographic_size;
                float current_thickness = orthographic_size * thickness;

                lineMaterial.SetPass(0);

                GL.PushMatrix();
                //GL.LoadOrtho();            

                GL.Begin(GL.QUADS);
                Vector2 startPoint;
                Vector2 endPoint;

                var diffx = Mathf.Abs((begin.x - end.x));
                var diffy = Mathf.Abs((begin.y - end.y));

                if (diffx > diffy)
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
                }

                var angleToX = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
                var angleToY = Mathf.Atan2(endPoint.x - startPoint.x, -(endPoint.y - startPoint.y));

               

                var cosToXaxis = Mathf.Cos(angleToX);
                var cosToYaxis = Mathf.Cos(angleToY );
                var sinToXaxis = Mathf.Sin(angleToX);
                var sinToYaxis = Mathf.Sin(angleToY);

                var distance = Vector2.Distance(startPoint, endPoint);

                Vector3 p1, p2, p3, p4;
                if (dashed)
                {
                    
                   // float stepLenght = gap * Mathf.Sqrt(cosToXaxis* cosToXaxis + ratio* sinToXaxis* sinToXaxis);
                    var totalDashes = distance / current_gap;
                    var stepByX = current_gap * cosToXaxis;
                    var stepByY = current_gap * sinToXaxis;
                    //var stepByY = ratio * gap * sinToXaxis;
                    Vector2 step = new Vector2(stepByX, stepByY);
                    Vector2 currentDashStartPoint = startPoint;
                    for (int n =0; n<=totalDashes; n++)
                    {

                        p1.x = currentDashStartPoint.x - current_thickness * cosToYaxis;
                        p1.y = currentDashStartPoint.y - current_thickness * sinToYaxis;

                        p2.x = currentDashStartPoint.x + current_thickness * cosToYaxis;
                        p2.y = currentDashStartPoint.y + current_thickness * sinToYaxis;

                        p3.x = p2.x + current_dashLength * cosToXaxis;
                        //p3.y = p2.y + ratio*dashLength * sinToXaxis;
                        p3.y = p2.y + current_dashLength * sinToXaxis;

                        p4.x = p1.x + current_dashLength * cosToXaxis;
                        //p4.y = p1.y + ratio * dashLength * sinToXaxis;
                        p4.y = p1.y + current_dashLength * sinToXaxis;

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

