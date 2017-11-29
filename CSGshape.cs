using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CSGshape {
    private const float smallDelta = .0001f;
    private List<CSGsegment> segments;
    public CSGshape(List<Vector2> points)
    {
        segments = new List<CSGsegment>();
        CSGsegment lastSegment = null;
        Vector2 prev = points[points.Count];
        foreach(var i in points)
        {
            var currSegment = new CSGsegment(prev, i);
            segments.Add(currSegment);
            if (lastSegment != null)
            {
                currSegment.startSegment = lastSegment;
                lastSegment.endSegment = currSegment;
            }
            lastSegment = currSegment;
            prev = i;
        }
        var first = segments[0];
        first.startSegment = lastSegment;
        lastSegment.endSegment = first;
    }
    private bool isPointInside(Vector2 point)
    {
        throw new NotImplementedException();
    }
    //external segments must ALWAYS be in order
    private List<CSGsegment> getExternalSegments(CSGshape other)
    {
        throw new NotImplementedException();
    }
    //make sure to only wiggle a little bit, according to some small constant 
    public bool getDirection(CSGsegment seg,bool isEntering,Vector2 intersectionPoint)
    {
        var startWiggle = intersectionPoint + smallDelta * seg.start;
        var endWiggle = intersectionPoint + smallDelta * seg.end;
        var isStart = isPointInside(startWiggle);
        var isEnd = isPointInside(endWiggle);
        if (isStart && isEnd)
        {
            throw new Exception("Delta too large: error encountered");
        } else if (!isStart && !isEnd)
        {
            throw new Exception("This really shouldn't happen");
        }
        if (isEntering)
        {
            return !isStart;
        } else
        {
            return !isEnd;
        }
    }
    //returns empty list if nothing is left
    public List<CSGshape> not(CSGshape other)
    {
        //start with a point on me thats not in the other shape
        List<CSGshape> retr = new List<CSGshape>();
        List<CSGsegment> externalSegments = getExternalSegments(other);
        if (externalSegments.Count == 0)//other completely surrounds this
        {
            return retr;
        } else if (externalSegments.Count == this.segments.Count)//this completely surrounds other
        {
            throw new NotImplementedException();
        } else
        {
            while (externalSegments.Count > 0)
            {
                List<Vector2> newPoints = new List<Vector2>();
                bool isMovingForward = true;
                var first = externalSegments[0];//better to remove at start or end? look this up
                var curr = first;
                externalSegments.RemoveAt(0);
                while (true)
                {
                    var partialPath = curr.ridePathUntilIntersection(other, externalSegments, isMovingForward, first);
                    foreach (var i in partialPath.first)
                    {
                        newPoints.Add(i.getPoint(isMovingForward));
                    }
                    if (partialPath.second == null) { break; }
                    var intersectionPoint = partialPath.first[partialPath.first.Count - 1].intersectionPoint(partialPath.second);
                    //now we must enter this shape, so test the two points
                    isMovingForward = this.getDirection(partialPath.second,true,intersectionPoint);
                    curr = new CSGsegment(intersectionPoint, partialPath.second.getPoint(!isMovingForward));//!isMovingForward so we get end instead of start
                    curr.startSegment = partialPath.second.startSegment;
                    curr.endSegment = partialPath.second.endSegment;
                    ////////////////////////now inside this
                    partialPath = curr.ridePathUntilIntersection(this, externalSegments, isMovingForward, null);//null because it can't end inside
                    foreach (var i in partialPath.first)
                    {
                        newPoints.Add(i.getPoint(isMovingForward));
                    }
                    intersectionPoint = partialPath.first[partialPath.first.Count - 1].intersectionPoint(partialPath.second);
                    //now we gotta leave this shape
                    isMovingForward = other.getDirection(partialPath.second,false,intersectionPoint);
                    curr = new CSGsegment(intersectionPoint, partialPath.second.getPoint(!isMovingForward));//!isMovingForward so we get end
                    curr.startSegment = partialPath.second.startSegment;
                    curr.endSegment = partialPath.second.endSegment;

                }
                retr.Add(new CSGshape(newPoints));
            }
            return retr;
        }
    }
}

public class CSGsegment
{
    public Vector2 start;
    public Vector2 end;
    public CSGsegment startSegment;
    public CSGsegment endSegment;
    public CSGsegment(Vector2 s, Vector2 e)
    {
        start = s;
        end = e;
    }
    //returns a list of the segments of the path and then the segment on the other shape which ended the path, also ends when it reaches the start
    public Tuple<List<CSGsegment>,CSGsegment> ridePathUntilIntersection(CSGshape shapeToRide,List<CSGsegment> externalSegments,bool isMovingForward,CSGsegment first)
    {
        //the tuple.second is null if it reached the "first" element
        throw new NotImplementedException();
    }
    //note: endpoints shouldn't count as intersections. Double check
    public bool doesIntersect(CSGsegment other)
    {
        throw new NotImplementedException();
    }

    public CSGsegment next(bool isMovingForward)
    {
        if (startSegment==null || endSegment == null)
        {
            throw new Exception();
        }
        if (isMovingForward)
        {
            return endSegment;
        } else
        {
            return startSegment;
        }
    }
    public Vector2 getPoint(bool isMovingForward)
    {
        if (isMovingForward)
        {
            return start;
        } else
        {
            return end;
        }
    }

    public Vector2 intersectionPoint(CSGsegment other)
    {
        throw new NotImplementedException();
    }

    //returns null for no intersection
    public CSGsegment getClosestIntersectingSegment(CSGshape shape)
    {
        throw new NotImplementedException();
    }
}

public class Tuple<S,T>
{
    public S first;
    public T second;
}
public class Tuple<S,T,U>
{
    public S first;
    public T second;
    public U third;
}
