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
            static public float thickness = 0.002f;
            static public float gap = 0.5f; //gap = dashlength + shift
            static public float dashLength = 0.3f;
            static Material lineMaterial = new Material(Shader.Find("Unlit/SimpleColor"));
            static public Color LineColor
            {
                get { return lineMaterial.color; }
                set
                {
                    lineMaterial.color = value;
                    lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            //Если камера не будет наведена на то место через которое проходит линия, то ничего отрисовываться не будет
            static public void TestDraw(Vector2 begin, Vector2 end)
            {
                lineMaterial.SetPass(0);
                GL.PushMatrix();
                GL.Begin(GL.LINES);
                GL.Color(Color.red);
                GL.Vertex(begin);
                GL.Color(Color.red);
                GL.Vertex(end);
                GL.End();
                GL.PopMatrix();
            }


            static public void DrawLine(Vector2 begin, Vector2 end, float orthographic_size = 1, bool dashed = false)
            {
                float current_gap = gap* orthographic_size;
                float current_dashLength = dashLength* orthographic_size;
                float current_thickness =  thickness*orthographic_size;

                lineMaterial.SetPass(0);

                GL.PushMatrix();
                //GL.LoadOrtho();            

                GL.Begin(GL.QUADS);
                Vector2 startPoint;
                Vector2 endPoint;

                //Блок отвечающий за направление откуда куда рисовать
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
                if (temp < 1e-4) cosToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToXaxis = cosToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(cosToYaxis);
                if (temp < 1e-4) cosToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToYaxis = cosToYaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToXaxis);
                if ( temp < 1e-4) sinToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToXaxis = sinToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToYaxis);
                if (temp < 1e-4) sinToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToYaxis = sinToYaxis > 0 ? 1 : -1;

                var distance = Vector2.Distance(startPoint, endPoint);

                Vector3 p1, p2, p3, p4;
                if (dashed)
                {
                    
                    //float stepLenght = gap * Mathf.Sqrt(cosToXaxis* cosToXaxis + ratio* sinToXaxis* sinToXaxis);
                    float totalDashes = distance / current_gap;
                    float stepByX = current_gap * cosToXaxis;
                    float stepByY = current_gap * sinToXaxis;
                   // float stepByY = ratio * gap * sinToXaxis;
                    Vector2 step = new Vector2(stepByX, stepByY);
                    Vector2 currentDashStartPoint = new Vector2(startPoint.x + current_dashLength/2 * cosToXaxis, startPoint.y + current_dashLength/2 * sinToXaxis) ;
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

            static public void DrawLineOrtho(Vector2 begin, Vector2 end, bool dashed = false, float ratio = 1)
            {
                float current_gap = gap; //* orthographic_size;
                float current_dashLength = dashLength; //* orthographic_size;
                float current_thickness = thickness; //* orthographic_size;

                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.LoadOrtho();            

                GL.Begin(GL.QUADS);
                Vector2 startPoint;
                Vector2 endPoint;

                startPoint = begin;
                endPoint = end;

                float angleToX = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
                float angleToY = Mathf.Atan2(endPoint.x - startPoint.x, -(endPoint.y - startPoint.y));



                float cosToXaxis = Mathf.Cos(angleToX);
                float cosToYaxis = Mathf.Cos(angleToY);

                float sinToXaxis = Mathf.Sin(angleToX);
                float sinToYaxis = Mathf.Sin(angleToY);

                float temp = Mathf.Abs(cosToXaxis);
                if (temp < 1e-4) cosToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToXaxis = cosToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(cosToYaxis);
                if (temp < 1e-4) cosToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToYaxis = cosToYaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToXaxis);
                if (temp < 1e-4) sinToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToXaxis = sinToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToYaxis);
                if (temp < 1e-4) sinToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToYaxis = sinToYaxis > 0 ? 1 : -1;

                var distance = Vector2.Distance(startPoint, endPoint);

                Vector3 p1, p2, p3, p4;
                if (dashed)
                {   
                    float totalDashes = distance / current_gap;
                    float stepByX = current_gap * cosToXaxis;
                    float stepByY = ratio * gap * sinToXaxis;
                    Vector2 step = new Vector2(stepByX, stepByY);
                    Vector2 currentDashStartPoint = new Vector2(startPoint.x + current_dashLength / 2 * cosToXaxis, startPoint.y + current_dashLength / 2 * sinToXaxis);
                    for (int n = 0; n <= totalDashes; n++)
                    {

                        p1.x = currentDashStartPoint.x - current_thickness * cosToYaxis;
                        p1.y = currentDashStartPoint.y - current_thickness * sinToYaxis;

                        p2.x = currentDashStartPoint.x + current_thickness * cosToYaxis;
                        p2.y = currentDashStartPoint.y + current_thickness * sinToYaxis;

                        p3.x = p2.x + current_dashLength * cosToXaxis;
                        p3.y = p2.y + ratio*dashLength * sinToXaxis;

                        p4.x = p1.x + current_dashLength * cosToXaxis;
                        p4.y = p1.y + ratio * dashLength * sinToXaxis;

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
                    p3.y = p2.y + distance * sinToXaxis;

                    p4.x = p1.x + distance * cosToXaxis;
                    p4.y = p1.y + distance * sinToXaxis;

                    GL.Vertex3(p1.x, p1.y, 1);
                    GL.Vertex3(p2.x, p2.y, 1);
                    GL.Vertex3(p3.x, p3.y, 1);
                    GL.Vertex3(p4.x, p4.y, 1);
                }



                GL.End();
                GL.PopMatrix();
            }

            static public void DrawLine(Vector2 begin, Vector2 end, bool dashed = false, float thickness = 1, float gap = 16, float dashLength = 11)
            {
                float current_gap = gap ;
                float current_dashLength = dashLength;
                float current_thickness = thickness;

                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.LoadPixelMatrix();         

                GL.Begin(GL.QUADS);
                Vector2 startPoint;
                Vector2 endPoint;

                //Блок отвечающий за направление откуда куда рисовать
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
                float cosToYaxis = Mathf.Cos(angleToY);

                float sinToXaxis = Mathf.Sin(angleToX);
                float sinToYaxis = Mathf.Sin(angleToY);

                float temp = Mathf.Abs(cosToXaxis);
                if (temp < 1e-4) cosToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToXaxis = cosToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(cosToYaxis);
                if (temp < 1e-4) cosToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) cosToYaxis = cosToYaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToXaxis);
                if (temp < 1e-4) sinToXaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToXaxis = sinToXaxis > 0 ? 1 : -1;
                temp = Mathf.Abs(sinToYaxis);
                if (temp < 1e-4) sinToYaxis = 0;
                if (temp >= (1 - (1e-4)) && temp < 1) sinToYaxis = sinToYaxis > 0 ? 1 : -1;

                var distance = Vector2.Distance(startPoint, endPoint);

                Vector3 p1, p2, p3, p4;
                if (dashed)
                {

                    //float stepLenght = gap * Mathf.Sqrt(cosToXaxis* cosToXaxis + ratio* sinToXaxis* sinToXaxis);
                    float totalDashes = distance / current_gap;
                    float stepByX = current_gap * cosToXaxis;
                    float stepByY = current_gap * sinToXaxis;
                    // float stepByY = ratio * gap * sinToXaxis;
                    Vector2 step = new Vector2(stepByX, stepByY);
                    Vector2 currentDashStartPoint = new Vector2(startPoint.x + current_dashLength / 2 * cosToXaxis, startPoint.y + current_dashLength / 2 * sinToXaxis);
                    for (int n = 0; n <= totalDashes; n++)
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
                    p3.y = p2.y + distance * sinToXaxis;

                    p4.x = p1.x + distance * cosToXaxis;
                    p4.y = p1.y + distance * sinToXaxis;

                    GL.Vertex3(p1.x, p1.y, 1);
                    GL.Vertex3(p2.x, p2.y, 1);
                    GL.Vertex3(p3.x, p3.y, 1);
                    GL.Vertex3(p4.x, p4.y, 1);
                }



                GL.End();
                GL.PopMatrix();
            }

            //Рисует линию толщиной 1 пиксель. 
            //dashed - будет ли отрисована разрывная линия, если да, 
            //то dash_length - длина чёрточки, free_space_length -длина разрыва, drawDash - начинать ли рисовать с чёрточки
            static public void DrawOnePixelLine(Vector2 beginPoint, Vector2 endPoint, bool dashed = false, float dash_length = 11, float free_space_length = 6, bool drawDash = true)
            {
                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.LoadPixelMatrix();

                GL.Begin(GL.LINES);
                if(!dashed)
                {
                    GL.Vertex(beginPoint);
                    GL.Vertex(endPoint);
                }
                else
                {
                    Vector2 line_length = endPoint - beginPoint;
                    float hypotenuse = line_length.magnitude;
                    if (hypotenuse > 0)
                    {
                        Vector2 currentPoint = beginPoint;
                        
                        float sin = line_length.y / hypotenuse;
                        float cos = line_length.x / hypotenuse;

                        if (Mathf.Abs(cos) < 1e-4) cos = 0;
                        if (Mathf.Abs(cos) >= (1 - (1e-4)) && Mathf.Abs(cos) < 1) cos = cos > 0 ? 1 : -1;

                        if (Mathf.Abs(sin) < 1e-4) sin = 0;
                        if (Mathf.Abs(sin) >= (1 - (1e-4)) && Mathf.Abs(sin) < 1) sin = sin > 0 ? 1 : -1;

                        line_length = new Vector2(cos, sin);
                        
                        
                        Vector2 addition = line_length * (drawDash? dash_length : free_space_length);
                        while (endPoint != currentPoint && (endPoint - currentPoint - addition).sqrMagnitude / (endPoint- currentPoint).sqrMagnitude <1)
                        {
                            if (drawDash)
                            {
                                GL.Vertex(currentPoint);
                                currentPoint += addition;
                                GL.Vertex(currentPoint);
                                drawDash = false;
                                addition = line_length *  free_space_length;
                            }
                            else
                            {
                                currentPoint += addition;
                                addition = line_length * dash_length;
                                drawDash = true;
                            }
                        }
                        if ((endPoint - currentPoint - 0.001f*line_length).sqrMagnitude / (endPoint - currentPoint).sqrMagnitude <= 1)
                        {
                            GL.Vertex(currentPoint);
                            GL.Vertex(endPoint);
                        }
                    }
                }
                GL.End();

                GL.PopMatrix();
                
            }

            static public void DrawRectangle(Vector2 leftAngle, Vector2 rightAngle, Color color)
            {
                LineColor = color;
                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.LoadPixelMatrix();
                GL.Begin(GL.QUADS);

                GL.Vertex3(leftAngle.x,leftAngle.y, 1);
                GL.Vertex3(leftAngle.x,rightAngle.y, 1);
                GL.Vertex3(rightAngle.x,rightAngle.y, 1);
                GL.Vertex3(rightAngle.x,leftAngle.y, 1);

                GL.End();

                GL.PopMatrix();
            }

        }
    }

}

