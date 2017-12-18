using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;


public class CollisionHull
{
	public CSGshape externalHull;
	public List<CSGshape> holes = new List<CSGshape>();
	public void AndAll()
	{

	}
}

public class CSGshape {
	private const float smallDelta = .0001f;
	private const float largePositive = 1000;
	public List<CSGsegment> segments;

	/*public static bool operator ==(CSGshape a,CSGshape b)
	{
		CSGsegment first = null;
		CSGsegment other = null;
		if (a.isClockwise() != b.isClockwise())
		{
			throw new Exception();//slow, remove eventually
		}
		foreach (var j in a.segments)
		{
			foreach (var i in b.segments)
			{
				if (j.start == i.start)
				{
					first = j;
					other = i;
					break;
				}
			}
		}
		if (first == null)
		{
			return false;
		}
		var curr = first;
		var otherCurr = other;
		do
		{
			if (curr.start != otherCurr.start)
			{
				return false;
			}
			otherCurr = otherCurr.endSegment;
			curr = curr.endSegment;
		} while (curr!=first);
		return true;
	}*/

	//such a trash function to make sure 2 identical shapes are on top of each other
	/*public static List<CollisionHull> prune(List<CollisionHull> existingShapes, List<CollisionHull> listToPrune)
	{
		listToPrune.AddRange(existingShapes);
		listToPrune = removeDups(listToPrune);
		listToPrune.RemoveAll(a => existingShapes.Contains(a));
		return listToPrune;
	}

	public static List<CollisionHull> removeDups(List<CollisionHull> list)
	{
		List<CollisionHull> retr = new List<CollisionHull>();
		//list.RemoveAll(a => list.FindAll(b=>b.externalHull==a).Count()>1);
		foreach (var i in list)
		{
			if (retr.Count == 0)
			{
				retr.Add(i);
			}
			else
			{
				foreach (var j in retr)
				{
					if (i.externalHull == j.externalHull)
					{
						break;
					}
					else if (j == retr[retr.Count - 1])
					{
						retr.Add(i);
						break;
					}
				}
			}
		}
		return retr;
	}
	public static bool operator !=(CSGshape a,CSGshape b)
	{
		return !(a == b);	
	}*/
	public CSGshape(List<Vector2> points)
	{
		segments = new List<CSGsegment>();
		CSGsegment lastSegment = null;
		Vector2 prev = points[points.Count - 1];
		foreach (var i in points)
		{
			var currSegment = new CSGsegment(prev, i);
			//Debug.Log(currSegment.Letter);
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
	public bool doesIntersect(CSGsegment seg)
	{
		foreach (var i in this.segments)
		{
			if (i.doesIntersect(seg)) {
				return true;
			}
		}
		return false;
	}
	//make sure to only wiggle a little bit, according to some small constant 
	public bool getDirection(CSGsegment seg, bool isEntering, Vector2 intersectionPoint)
	{
		var startWiggle = intersectionPoint + smallDelta * (seg.start - intersectionPoint).normalized;
		var endWiggle = intersectionPoint + smallDelta * (seg.end - intersectionPoint).normalized;
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
	public bool isClockwise()
	{
		float sum = 0;
		var i = segments[0];
		do
		{
			sum += (i.end.x - i.start.x) * (i.end.y + i.start.y);
			i = i.endSegment;
		} while (i != segments[0]);
		return sum > 0;
	}
	public void antiRotate()//doesn't reorder segments in the list :(
	{
		foreach (var i in segments)
		{
			var temp = i.startSegment;
			i.startSegment = i.endSegment;
			i.endSegment = temp;
		}
	}

	/*public static List<CollisionHull> andAll(List<CollisionHull> hulls)
	{
		List<CollisionHull> remainingToTest = new List<CollisionHull>();
		List<CollisionHull> otherRemaining = new List<CollisionHull>();
		List<CollisionHull> retr = new List<CollisionHull>();
		while (remainingToTest.Count > 0)
		{
			var curr = remainingToTest[remainingToTest.Count-1];
			remainingToTest.RemoveAt(remainingToTest.Count - 1);
			otherRemaining.Clear();
			foreach (var i in remainingToTest)
			{
				var result = not(curr.externalHull, i.externalHull, false, curr.holes);
				if (result.Count == 1) {//they were able to merge
					curr = result[0].externalHull;
				} else
				{
					otherRemaining.Add(i);
				}
			}
			retr.Add(curr);
			remainingToTest = otherRemaining;
		}
		return retr;
	
	}*/

	public static List<CSGshape> andAll(List<CSGshape> shapes)
	{
		List<CSGshape> remainingToTest = new List<CSGshape>();
		List<CSGshape> otherRemaining = new List<CSGshape>();
		List<CSGshape> retr = new List<CSGshape>();
		while (remainingToTest.Count > 0)
		{
			var curr = remainingToTest[remainingToTest.Count-1];
			remainingToTest.RemoveAt(remainingToTest.Count - 1);
			otherRemaining.Clear();
			foreach (var i in remainingToTest)
			{
				bool none;
				var result = not(curr, i, false,out none,null);
				if (result.Count == 1) {//they were able to merge
					curr = result[0].externalHull;
				} else
				{
					otherRemaining.Add(i);
				}
			}
			retr.Add(curr);
			remainingToTest = otherRemaining;
		}
		return retr;
	}
    //collision hull.hull or whatever is null if nothing is left
    public static List<CollisionHull> not(CSGshape shape,CSGshape other, bool isNot,out bool usedFlag,List<CSGshape> holes=null)
    {
		//start with a point on me thats not in the other shape
		//List<CSGshape> retr = new List<CSGshape>();
		List<CollisionHull> hulls = new List<CollisionHull>();
        List<CSGsegment> externalSegments = shape.getExternalSegments(other);
		usedFlag = !isNot;
		if (other.segments.TrueForAll(i=>!shape.doesIntersect(i)) && !isNot)
		{
			var foo = new CollisionHull();
			foo.externalHull = shape;
			foo.holes = holes;
			/*var bar = new CollisionHull();
			bar.externalHull = other;*/
			hulls.Add(foo);
			//hulls.Add(bar);
			usedFlag = false;
			return hulls;
		}
		else if (externalSegments.Count == 0)//other completely surrounds this
		{
			if (isNot)
			{
				return hulls;
			}
			else
			{
				hulls.Add(new CollisionHull());
				hulls[0].externalHull = other;
				return hulls;
			}
		}
		else if (externalSegments.Count == shape.segments.Count && other.segments.TrueForAll(a => shape.isPointInside(a.start)))//this completely surrounds other
		{
			if (isNot)
			{
				var foo = new List<CSGshape>();
				foo.Add(other);
				if (holes != null)
				{
					foo.AddRange(holes);
				}
				foo = andAll(foo);
				var hull = new CollisionHull();
				hull.externalHull = shape;
				hull.holes.Add(other);
				hulls.Add(hull);
			}
			else
			{
				hulls.Add(new CollisionHull());
				hulls[0].externalHull = shape;
			}
			return hulls;
			//throw new NotImplementedException();
			/*float closestDist = float.PositiveInfinity;
            CSGsegment closestItem = null;
            CSGsegment closestInnerItem = null;
            if (!other.isClockwise())
            {
                other.antiRotate();
            }
            foreach (var i in shape.segments)
            {
                foreach (var j in other.segments)
                {
                    var dist = (i.end - j.start).magnitude;
                    if (dist < closestDist)
                    {
                        closestItem = i;
                        closestInnerItem = j;
                        closestDist = dist;
                    }
                }
            }
            var enter = new CSGsegment(closestItem.end, closestInnerItem.start);
            var exit = new CSGsegment(closestInnerItem.start, closestItem.end);
            var oldnext = closestItem.endSegment;
            var oldinnernext = closestInnerItem.startSegment;
            oldnext.startSegment = exit;
            closestItem.endSegment = enter;
            enter.endSegment = closestInnerItem;
            exit.endSegment = oldnext;
            oldinnernext.endSegment = exit;
            enter.startSegment = closestItem;
            closestInnerItem.startSegment = enter;
            exit.startSegment = oldinnernext;

            CSGsegment curr = enter;
            List<Vector2> newPoints = new List<Vector2>();
            do
            {
                newPoints.Add(curr.getPoint(true));
                curr = curr.next(true);
            } while (curr!=enter);
            retr.Add(new CSGshape(newPoints));
            return retr;*/
		}
		else
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
					if (counter > 100)//remove this?
					{
						throw new Exception("Linerider exceeded 100 steps");
						//break;
					}
					counter++;
					var partialPath = curr.ridePathUntilIntersection(other, externalSegments, isMovingForward, first, isStart);
					foreach (var i in partialPath.first)
					{
						newPoints.Add(i.getPoint(isMovingForward));
					}
					if (partialPath.second == null) { break; }
					var intersectionPoint = partialPath.first[partialPath.first.Count - 1].intersectionPoint(partialPath.second);
					isMovingForward = shape.getDirection(partialPath.second, isNot, intersectionPoint);//toggle this to add instead of subtract
					isStart = false;
					//now we must enter this shape, so test the two points
					var exitPoint = partialPath.second.getPoint(!isMovingForward);//!isMovingForward so we get end instead of start
					var entrancePoint = intersectionPoint + smallDelta * (exitPoint - intersectionPoint).normalized;
					if (isMovingForward)
					{
						curr = new CSGsegment(entrancePoint, exitPoint);
					}
					else
					{
						curr = new CSGsegment(exitPoint, entrancePoint);
					}
					curr.startSegment = partialPath.second.startSegment;
					curr.endSegment = partialPath.second.endSegment;
					////////////////////////now inside this
					partialPath = curr.ridePathUntilIntersection(shape, externalSegments, isMovingForward, null);//null because it can't end inside
					foreach (var i in partialPath.first)
					{
						newPoints.Add(i.getPoint(isMovingForward));
					}
					intersectionPoint = partialPath.first[partialPath.first.Count - 1].intersectionPoint(partialPath.second);
					isMovingForward = other.getDirection(partialPath.second, false, intersectionPoint);
					//now we gotta leave this shape
					exitPoint = partialPath.second.getPoint(!isMovingForward);//!isMovingForward so we get end instead of start
					entrancePoint = intersectionPoint + smallDelta * (exitPoint - intersectionPoint).normalized;
					if (isMovingForward)
					{
						curr = new CSGsegment(entrancePoint, exitPoint);
					}
					else
					{
						curr = new CSGsegment(exitPoint, entrancePoint);
					}
					curr.startSegment = partialPath.second.startSegment;
					curr.endSegment = partialPath.second.endSegment;

				}
				var foo = new CollisionHull();
				var newShape = new CSGshape(newPoints);
				foo.externalHull = newShape;
				hulls.Add(foo);
			}
			return hulls;
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
    public float getAngle()//must always be between 0 and 360 or 0 and pi
    {
        return Mathf.Atan2(end.y-start.y,end.x-start.x);
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
            //Debug.Log(curr.Letter);
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
        if (this.end.x == this.start.x && other.start.x == other.end.x)
        {
            throw new Exception("Vertical overlapping lines");
            //Debug.Log("Vertical overlapping lines");
            //return this.end;
        }
        if (this.end.x == this.start.x)
        {
            return new Vector2(this.start.x,other.getSlope()*this.start.x+other.getYInter());
        } else if (other.end.x == other.start.x)    
        {
            return new Vector2(other.start.x,this.getSlope()*other.start.x+this.getYInter());
        }
        var num = this.getYInter() - other.getYInter();
        var denom = other.getSlope() - this.getSlope();
        var x = num / denom;
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
