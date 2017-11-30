using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public class CSGshape {
    private const float smallDelta = .001f;
    private const float largePositive = 100;
    public List<CSGsegment> segments;
    public CSGshape(List<Vector2> points)
    {
        segments = new List<CSGsegment>();
        CSGsegment lastSegment = null;
        Vector2 prev = points[points.Count-1];
        foreach(var i in points)
        {
            var currSegment = new CSGsegment(prev, i);
            Debug.Log(currSegment.Letter);
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
        var collisionLine = new CSGsegment(point, new Vector2(largePositive, point.y));
        CSGsegment.LetterCounter--;
        int counter = 0;
        foreach (var i in this.segments)
        {
            if (collisionLine.doesIntersect(i))
            {
                counter++;
            }
        }
        return counter % 2 == 1;//odd number of intersections? then it is inside the shape
    }
    //external segments must ALWAYS be in order
    private List<CSGsegment> getExternalSegments(CSGshape other)
    {
        var retr = new List<CSGsegment>();
        foreach (var i in this.segments)
        {
            if (!other.isPointInside(i.start))
            {
                retr.Add(i);
            }
        }
        return retr;
    }
    //make sure to only wiggle a little bit, according to some small constant 
    public bool getDirection(CSGsegment seg,bool isEntering,Vector2 intersectionPoint)
    {
        var startWiggle = intersectionPoint + smallDelta * (seg.start-intersectionPoint).normalized;
        var endWiggle = intersectionPoint + smallDelta * (seg.end-intersectionPoint).normalized;
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
                var isStart = true;
                externalSegments.RemoveAt(0);
                int counter = 0;
                while (true)
                {
                    if (counter > 100)
                    {
                        break;
                    }
                    counter++;
                    var partialPath = curr.ridePathUntilIntersection(other, externalSegments, isMovingForward, first,isStart);
                    foreach (var i in partialPath.first)
                    {
                        newPoints.Add(i.getPoint(isMovingForward));
                    }
                    isStart = false;
                    if (partialPath.second == null) { break; }
                    var intersectionPoint = partialPath.first[partialPath.first.Count - 1].intersectionPoint(partialPath.second);
                    //now we must enter this shape, so test the two points
                    isMovingForward = this.getDirection(partialPath.second,true,intersectionPoint);
                    var entrancePoint = partialPath.second.getPoint(!isMovingForward);//!isMovingForward so we get end instead of start
                    curr = new CSGsegment(intersectionPoint+smallDelta*(entrancePoint-intersectionPoint).normalized,entrancePoint);
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
                    entrancePoint = partialPath.second.getPoint(!isMovingForward);//!isMovingForward so we get end instead of start
                    curr = new CSGsegment(intersectionPoint+smallDelta*(entrancePoint-intersectionPoint).normalized, entrancePoint);
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
    public static char LetterCounter = 'a';
    public char Letter;
    public CSGsegment(Vector2 s, Vector2 e)
    {
        Letter = LetterCounter;
        LetterCounter += (char)1;
        start = s;
        end = e;
    }
    //returns a list of the segments of the path and then the segment on the other shape which ended the path, also ends when it reaches the start
    public Tuple<List<CSGsegment>,CSGsegment> ridePathUntilIntersection(CSGshape shapeToIntersect,List<CSGsegment> externalSegments,bool isMovingForward,CSGsegment first,bool isStart=false)
    {
        var retr = new Tuple<List<CSGsegment>, CSGsegment>();
        var curr = this;
        var retrList = new List<CSGsegment>();
        retr.first = retrList;
        retr.second = null;
        while (curr != first || isStart)
        {
            Debug.Log(curr.Letter);
            isStart = false;
            retrList.Add(curr);
            if (externalSegments.Contains(curr))//slow linear search
            {
                externalSegments.Remove(curr);
            }
            var intersection = curr.getClosestIntersectingSegment(shapeToIntersect, isMovingForward);
            if (intersection == null)
            {
                curr = curr.next(isMovingForward);
            } else
            {
                retr.second = intersection;
                return retr; 
            }
        }
        return retr;
    }

    ///not my code
    private bool onSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }
    private float orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) -
                  (q.x - p.x) * (r.y - q.y);
        if (val == 0) return 0; 
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }
    private bool doIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        float o1 = orientation(p1, q1, p2);
        float o2 = orientation(p1, q1, q2);
        float o3 = orientation(p2, q2, p1);
        float o4 = orientation(p2, q2, q1);
        if (o1 != o2 && o3 != o4)
            return true;
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;
        return false;
    }
    ///end of not my code

    public bool doesIntersect(CSGsegment other)
    {
        return doIntersect(this.start,this.end,other.start,other.end);
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

    public float getSlope()
    {
        return (end.y - start.y) / (end.x-start.x);
    }

    public float getYInter()
    {
        return start.y - getSlope() * start.x;
    }
    public string ToString
    {
        get
        {
            return "(" + this.start + "," +this.end+ ")";
        }
    }

    public Vector2 intersectionPoint(CSGsegment other)//can optimize by removing redundant slope&yinter calculations
    {
        var x = (this.getYInter() - other.getYInter()) / (other.getSlope()-this.getSlope());
        return new Vector2(x,x*this.getSlope()+this.getYInter());
    }

    //returns null for no intersection
    public CSGsegment getClosestIntersectingSegment(CSGshape shape, bool isMovingForward)
    {
        var candidates = new List<CSGsegment>();
        foreach (var i in shape.segments)
        {
            if (i.doesIntersect(this))
            {
                candidates.Add(i);
            }
        }
        float currBestDist = float.PositiveInfinity;
        CSGsegment currBest = null;
        foreach (var i in candidates)
        {
            var point = i.intersectionPoint(this);
            var dist = (this.getPoint(isMovingForward) - point).magnitude;
            if (dist < currBestDist)
            {
                currBestDist = dist;
                currBest = i;
            }
        }
        return currBest;
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
