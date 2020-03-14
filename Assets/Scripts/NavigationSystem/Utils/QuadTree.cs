using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Quadtree by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
//Any object that you insert into the tree must implement this interface
public interface IQuadTreeObject
{
    Vector2 Position { get; }
    int Layer { get; }
    int SpatialGroup { get; set; }
}

public class QuadTree<T>
    where T : IQuadTreeObject
{
    private int m_maxObjectCount;
    private List<T> m_storedObjects;
    private Rect m_bounds;
    private QuadTree<T>[] cells;
    private int maxDepth = 10;
    private int m_depth;

    public QuadTree(int maxSize, Rect bounds, int depth = 0)
    {
        m_bounds = bounds;
        m_maxObjectCount = maxSize;
        cells = new QuadTree<T>[4];
        m_storedObjects = new List<T>(maxSize);
        m_depth = depth;
    }

    public void Insert(T objectToInsert)
    {

        if (cells[0] != null)
        {
            int iCell = GetCellToInsertObject(objectToInsert.Position);
            if (iCell > -1)
            {
                cells[iCell].Insert(objectToInsert);
            }
            return;
        }
        m_storedObjects.Add(objectToInsert);
        //Objects exceed the maximum count
        if (m_depth < maxDepth && m_storedObjects.Count > m_maxObjectCount)
        {
            Expand();
        }
    }
    public void Remove(T objectToRemove, Vector2 position)
    {
        if (ContainsLocation(position))
        {
            m_storedObjects.Remove(objectToRemove);
            if (cells[0] != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    //if (cells[i].ContainsLocation(position))
                    // {
                    cells[i].Remove(objectToRemove, position);
                    // }
                }
            }
            if (cells[0] != null && CountChild < m_maxObjectCount / 2)
            {
                Shrink();
            }
        }
    }

    void Expand()
    {
        //Split the quad into 4 sections
        if (cells[0] == null)
        {
            float subWidth = (m_bounds.width / 2f);
            float subHeight = (m_bounds.height / 2f);
            float x = m_bounds.x;
            float y = m_bounds.y;
            cells[0] = new QuadTree<T>(m_maxObjectCount, new Rect(x + subWidth, y, subWidth, subHeight), m_depth + 1);
            cells[1] = new QuadTree<T>(m_maxObjectCount, new Rect(x, y, subWidth, subHeight), m_depth + 1);
            cells[2] = new QuadTree<T>(m_maxObjectCount, new Rect(x, y + subHeight, subWidth, subHeight), m_depth + 1);
            cells[3] = new QuadTree<T>(m_maxObjectCount, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), m_depth + 1);
        }
        //Reallocate this quads objects into its children
        int i = m_storedObjects.Count - 1;
        while (i >= 0)
        {
            T storedObj = m_storedObjects[i];
            int iCell = GetCellToInsertObject(storedObj.Position);
            if (iCell > -1)
            {
                cells[iCell].Insert(storedObj);
            }
            m_storedObjects.RemoveAt(i);
            i--;
        }
    }
    void Shrink()
    {
        if (cells[0] != null)
        {
            RetrieveAllChild(ref m_storedObjects);
            for (int i = 0; i < 4; i++)
            {
                cells[i] = null;
            }
        }
    }
    public int Count
    {
        get
        {
            int size = m_storedObjects.Count + CountChild;
            return size;
        }
    }
    int CountChild
    {
        get
        {
            int size = 0;
            if (cells[0] != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    size += cells[i].Count;
                }
            }
            return size;
        }
    }

    public void RetrieveAll(ref List<T> container)
    {
        container.AddRange(m_storedObjects);
        RetrieveAllChild(ref m_storedObjects);
    }
    void RetrieveAllChild(ref List<T> container)
    {
        if (cells[0] != null)
        {
            for (int i = 0; i < 4; i++)
            {
                cells[i].RetrieveAll(ref container);
            }
        }
    }
    public List<T> RetrieveObjectsInArea(Rect area)
    {
        if (rectOverlap(m_bounds, area))
        {
            List<T> returnedObjects = new List<T>();
            for (int i = 0; i < m_storedObjects.Count; i++)
            {
                if (ContainsLocation(m_storedObjects[i].Position))
                {
                    returnedObjects.Add(m_storedObjects[i]);
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    List<T> cellObjects = cells[i].RetrieveObjectsInArea(area);
                    if (cellObjects != null)
                    {
                        returnedObjects.AddRange(cellObjects);
                    }
                }
            }
            return returnedObjects;
        }
        return null;
    }
    public void RetrieveObjectsInAreaNonAloc(Rect area, LayerMask mask, ref T[] returnedObjects, ref int count)
    {
        if (rectOverlap(m_bounds, area))
        {
            for (int i = 0; i < m_storedObjects.Count && returnedObjects.Length > count; i++)
            {
                if (ContainsLocation(m_storedObjects[i].Position) && LayerMaskExtensions.Contains(mask, m_storedObjects[i].Layer))
                {
                    returnedObjects[count++] = m_storedObjects[i];
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < 4 && returnedObjects.Length > count; i++)
                {
                    cells[i].RetrieveObjectsInAreaNonAloc(area, mask, ref returnedObjects, ref count);
                }
            }
        }
    }
    public void RetrieveObjectsInAreaNonAloc(Rect area, ref T[] returnedObjects, ref int count)
    {
        if (rectOverlap(m_bounds, area))
        {
            for (int i = m_storedObjects.Count - 1; i >= 0 && returnedObjects.Length > count; --i)
            {
                if (m_storedObjects[i] != null)
                {
                    try
                    {
                        if (ContainsLocation(m_storedObjects[i].Position))
                        {
                            returnedObjects[count++] = m_storedObjects[i];
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Bad actor indexing, at index " + i);
                    }
                }
                else
                {
                    m_storedObjects.RemoveAt(i);
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < 4 && returnedObjects.Length > count; i++)
                {
                    cells[i].RetrieveObjectsInAreaNonAloc(area, ref returnedObjects, ref count);
                }
            }
        }
    }

    // Clear quadtree
    public void Clear()
    {
        m_storedObjects.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null)
            {
                cells[i].Clear();
                cells[i] = null;
            }
        }
    }
    public bool ContainsLocation(Vector2 location)
    {
        return m_bounds.Contains(location);
    }
    private int GetCellToInsertObject(Vector2 location)
    {
        for (int i = 0; i < 4; i++)
        {
            if (cells[i].ContainsLocation(location))
            {
                return i;
            }
        }
        return -1;
    }
    public int GetCell(Vector2 location, int current = 0)
    {
        if (ContainsLocation(location))
        {
            if (cells[0] != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (cells[i].ContainsLocation(location))
                    {
                        return cells[i].GetCell(location, current * 4 + i);
                    }
                }
            }
            else
            {
                return current;
            }
        }
        return -1;
    }
    static bool valueInRange(float value, float min, float max)
    { return (value >= min) && (value <= max); }

    static bool rectOverlap(Rect A, Rect B)
    {
        bool xOverlap = valueInRange(A.x, B.x, B.x + B.width) ||
                        valueInRange(B.x, A.x, A.x + A.width);

        bool yOverlap = valueInRange(A.y, B.y, B.y + B.height) ||
                        valueInRange(B.y, A.y, A.y + A.height);

        return xOverlap && yOverlap;
    }
    public void DrawDebug()
    {
        UnityEditor.Handles.Label(new Vector3(m_bounds.center.x, 0, m_bounds.center.y), Count.ToString());
        Gizmos.DrawLine(new Vector3(m_bounds.x, 0, m_bounds.y), new Vector3(m_bounds.x, 0, m_bounds.y + m_bounds.height));
        Gizmos.DrawLine(new Vector3(m_bounds.x, 0, m_bounds.y), new Vector3(m_bounds.x + m_bounds.width, 0, m_bounds.y));
        Gizmos.DrawLine(new Vector3(m_bounds.x + m_bounds.width, 0, m_bounds.y), new Vector3(m_bounds.x + m_bounds.width, 0, m_bounds.y + m_bounds.height));
        Gizmos.DrawLine(new Vector3(m_bounds.x, 0, m_bounds.y + m_bounds.height), new Vector3(m_bounds.x + m_bounds.width, 0, m_bounds.y + m_bounds.height));
        if (cells[0] != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                {
                    cells[i].DrawDebug();
                }
            }
        }
    }
}