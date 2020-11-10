using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Transactions;

public enum Dimension
{
    X,
    Y,
    Z
}

namespace quiet
{
    public struct VectorUtils
    {
        #region Square Distance
        /// <summary>
        /// Calculates the squared horizontal distance between two GameObjects
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>The squared distance between the two GameObjects</returns>
        public static float CalcSqDistance(GameObject obj1, GameObject obj2) => CalcSqDistance(obj1?.transform.position ?? Vector3.zero, obj2?.transform.position ?? Vector3.zero);

        /// <summary>
        /// Calculates the squared horizontal distance between two vectors
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns>The squared distance between the two GameObjects</returns>
        public static float CalcSqDistance(Vector3 pos1, Vector3 pos2) => CalcSqDistance(pos1.StripZDim(), pos2.StripZDim());

        public static float CalcSqDistance(Vector2 pos1, Vector2 pos2) => Mathf.Pow(pos1.x - pos2.x, 2) + Mathf.Pow(pos1.y - pos2.y, 2);

        /// <summary>
        /// Calculates the squared horizontal distance of each GameObject in target and the origin
        /// </summary>
        /// <param name="origin">The GameObject to compare every other GameObject to</param>
        /// <param name="targets">An array of every object to calculate the distance to the origin object</param>
        /// <returns>An array representing the squared distances between the origin and each GameObject in targets</returns>
        public static IEnumerable<float> CalcSqDistances(GameObject origin, IEnumerable<GameObject> targets) => targets.Select(target => CalcSqDistance(origin, target));

        public static IEnumerable<float> CalcSqDistances(Vector3 origin, IEnumerable<Vector3> targets) => targets.Select(target => CalcSqDistance(origin, target));

        public static IEnumerable<float> CalcSqDistances(Vector2 origin, IEnumerable<Vector2> targets) => targets.Select(target => CalcSqDistance(origin, target));
        #endregion

        #region Distance
        /// <summary>
        /// Calculates the distance between two object on the x,z plane
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>The distance between the two GameObjects</returns>
        public static float CalcDistance(GameObject obj1, GameObject obj2) => Mathf.Sqrt(CalcSqDistance(obj1, obj2));

        /// <summary>
        /// Calculates the distance between two points on the x,z plane
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns>The distance between the two GameObjects</returns>
        public static float CalcDistance(Vector3 pos1, Vector3 pos2) => Mathf.Sqrt(CalcSqDistance(pos1, pos2));

        public static float CalcDistance(Vector2 pos1, Vector2 pos2) => Mathf.Sqrt(CalcSqDistance(pos1, pos2));

        /// <summary>
        /// Calcultes the distance between each GameObject in target and the origin on the x,z plane
        /// </summary>
        /// <param name="origin">The GameObject to compare every other GameObject to</param>
        /// <param name="targets">An array of every object to calculate the distance to the origin object</param>
        /// <returns>An array representing the distances between the origin and each GameObject in targets</returns>
        public static float[] CalcDistances(GameObject origin, IEnumerable<GameObject> targets) => CalcSqDistances(origin, targets).Select(sqdistance => Mathf.Sqrt(sqdistance)).ToArray();

        public static float[] CalcDistances(Vector3 origin, IEnumerable<Vector3> targets) => CalcSqDistances(origin, targets).Select(s => Mathf.Sqrt(s)).ToArray();

        public static float[] CalcDistances(Vector2 origin, IEnumerable<Vector2> targets) => CalcSqDistances(origin, targets).Select(s => Mathf.Sqrt(s)).ToArray();
        #endregion

        #region Close Objects
        /// <summary>
        /// Finds every GameObject within a radius distance from origin
        /// </summary>
        /// <param name="origin">The GameObject representing the center of the bounding circle</param>
        /// <param name="targets">An array of objects to check if they are close to origin</param>
        /// <param name="distance">The radius of the circle to check within</param>
        /// <returns>An array of GameObjects that are within a radius distance from origin</returns>
        public static IEnumerable<GameObject> FindCloseObjects(GameObject origin, IEnumerable<GameObject> targets, float distance) => targets.Where((target) => CalcSqDistance(origin, target) < distance * distance).ToArray();

        /// <summary>
        /// Finds every Vector3 within a radius distance from origin
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="targets"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> FindCloseVec3s(Vector3 origin, IEnumerable<Vector3> targets, float distance) => targets.Where((target) => CalcSqDistance(origin, target) < distance * distance).ToArray();

        public static bool IsCloseVec3(Vector3 origin, Vector3 target, float distance) => CalcSqDistance(origin, target) < distance * distance;

        public static bool IsCloseVec2(Vector2 origin, Vector2 target, float distance) => CalcSqDistance(origin, target) < distance * distance;

        /// <summary>
        /// Determines whether there exists any GameObjects from targets less than or equal to distance away from origin
        /// </summary>
        /// <param name="origin">The GameObject representing the center of the bounding circle</param>
        /// <param name="targets">An array of objects to check if they are close to origin</param>
        /// <param name="distance">The radius of the circle to check within</param>
        /// <returns>True if there exists at least one GameObject, false otherwise</returns>
        public static bool AreCloseObjects(GameObject origin, IEnumerable<GameObject> targets, float distance) => FindCloseObjects(origin, targets, distance).Count() > 0;

        /// <summary>
        /// Determines the closest GameObject within targets to origin
        /// </summary>
        /// <param name="origin">The GameObject to check</param>
        /// <param name="targets">An array of every GameObject to compare against</param>
        /// <returns>A GameObject from within targets that is closest to origin</returns>
        public static GameObject FindClosest(GameObject origin, IEnumerable<GameObject> targets)
        {
            if (targets == null || !targets.Any())
                return null;

            // If origin is in targets, remove it
            targets = targets.Where((target) => target != origin);
            // Calculate the distance to each gameobject
            IEnumerable<float> distances = CalcSqDistances(origin, targets);
            // Determine the closest and return it
            return targets.ElementAt(Array.IndexOf(distances.ToArray(), distances.Aggregate((d1, d2) => d1 < d2 ? d1 : d2)));
        }

        /// <summary>
        /// Determines the closest GameObject within targets to origin and are within a distance distance from origin
        /// </summary>
        /// <param name="origin">The GameObject to check</param>
        /// <param name="targets">An array of every GameObject to compare against</param>
        /// <param name="distance">The radius of the circle to check within</param>
        /// <returns></returns>
        public static GameObject FindClosest(GameObject origin, GameObject[] targets, float distance) => FindClosest(origin, FindCloseObjects(origin, targets, distance));

        public static Vector3 FindClosest(Vector3 origin, IEnumerable<Vector3> targets)
        {
            if (targets == null || !targets.Any())
                return Vector3.zero;

            // If origin is in targets, remove it
            targets = targets.Where((target) => target != origin);
            // Calculate the distance to each gameobject
            IEnumerable<float> distances = CalcSqDistances(origin, targets);
            // Determine the closest and return it
            return targets.ElementAt(Array.IndexOf(distances.ToArray(), distances.Aggregate((d1, d2) => d1 < d2 ? d1 : d2)));
        }
        #endregion

        #region RandomPoint
        /// <summary>
        /// Determines a random point within the specified bounds
        /// </summary>
        /// <returns>A random Vector3</returns>
        public static Vector3 GetRandomPoint(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) => new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));

        /// <summary>
        /// Determines a random point along the horizontal plane at y=0
        /// </summary>
        /// <returns>A random Vector3</returns>
        public static Vector3 GetRandomPoint(float minX, float maxX, float minZ, float maxZ) => GetRandomPoint(minX, maxX, 0, 0, minZ, maxZ);

        /// <summary>
        /// Determines a random point along the horizontal plane at a specified y
        /// </summary>
        /// <param name="Y">Plane height</param>
        /// <returns>A random Vector3</returns>
        public static Vector3 GetRandomPoint(float minX, float maxX, float Y, float minZ, float maxZ) => GetRandomPoint(minX, maxX, Y, Y, minZ, maxZ);

        /// <summary>
        /// Determines a random point within the specified bounds
        /// </summary>
        /// <param name="X">X Bounds</param>
        /// <param name="Y">Y Bounds</param>
        /// <param name="Z">Z Bounds</param>
        /// <returns>A random Vector3</returns>
        public static Vector3 GetRandomPoint((float min, float max) X, (float min, float max) Y, (float min, float max) Z) => GetRandomPoint(X.min, X.max, Y.min, Y.max, Z.min, Z.max);

        /// <summary>
        /// Determines a random point along the horizontal plane at a specified y
        /// </summary>
        /// <param name="X">X Bounds</param>
        /// <param name="Y">Plane height</param>
        /// <param name="Z">Z Bounds</param>
        /// <returns>A random Vector3</returns>
        public static Vector3 GetRandomPoint((float min, float max) X, float Y, (float min, float max) Z) => GetRandomPoint(X.min, X.max, Y, Y, Z.min, Z.max);

        public static Vector3 GetRandomPoint_2D((float min, float max) X, (float min, float max) Y) => new Vector3(Random.Range(X.min, X.max), Random.Range(Y.min, Y.max));
        #endregion

        #region Average Direction
        /// <summary>
        /// Calculates the average direction of a collection of vectors
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static Vector3 CalcAvgDirection(IEnumerable<Vector3> vectors) => vectors.Aggregate((obj1, obj2) => obj1 + obj2).normalized;

        /// <summary>
        /// Calculates the average direction of a collection of gameobjects
        /// </summary>
        /// <param name="gObjects"></param>
        /// <returns></returns>
        public static Vector3 CalcAvgDirection(IEnumerable<GameObject> gObjects) => CalcAvgDirection(gObjects.Select((obj) => obj.transform.forward));
        #endregion
    }

    /// <summary>
    /// A collection of loosely typed global variables
    /// </summary>
    public struct Variables
    {
        /// <summary>
        /// All variables
        /// </summary>
        public static Dictionary<string, object> variables =
            new Dictionary<string, object>() {
                { "bounds", new Bounds(new Vector3(50, 0, 50), new Vector3(95, 95, 95)) },
                { "center", new Vector3(50, 0, 50) }
            };

        /// <summary>
        /// Adds a variable to the collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Add(string name, object value)
        {
            if (!variables.ContainsKey(name))
            {
                variables.Add(name, value);
            }
            else
            {
                variables[name] = value;
            }
        }

        /// <summary>
        /// Adds several variables to the collection
        /// </summary>
        /// <param name="vars"></param>
        public static void Add(IDictionary<string, object> vars)
        {
            foreach (KeyValuePair<string, object> variable in vars)
            {
                Add(variable.Key, variable.Value);
            }
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        public static bool Set(string name, object newVal)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = newVal;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the value of a variable given it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object Get(string name) => variables.ContainsKey(name) ? variables[name] : null;

        /// <summary>
        /// Returns the value of a variables casted to a given type given it's name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name) => (T)variables[name];

        /// <summary>
        /// Determines if there is an entry in variables with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>True if there is a variable</returns>
        public static bool HasVariable(string name) => variables.ContainsKey(name);

        #region Initialize
        /// <summary>
        /// If an variable doesn't exist with the given name, create one, set it's value to value and return it's value. if it already exists, just return it's value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Initialize(string name, object value)
        {
            if (!HasVariable(name))
            {
                variables[name] = value;
            }

            return variables[name];
        }

        /// <summary>
        /// Initiliazed a variabled with a given name and a value returned by a func
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>The value of the variable with name name</returns>
        public static object Initialize(string name, Func<object> value)
        {
            if (!HasVariable(name))
            {
                variables[name] = value.Invoke();
            }

            return variables[name];
        }
        #endregion

        #region Initialize Generic
        /// <summary>
        /// Same as initialized, but returns the value precasted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Initialize<T>(string name, T value)
        {
            if (!HasVariable(name))
            {
                variables[name] = value;
            }

            try
            {
                _ = (T)variables[name];
                return (T)variables[name];
            }
            catch (InvalidCastException e)
            {
                Debug.LogError("Variable " + name + " could not be retrieved as type " + typeof(T).FullName + "\n Full error:" + e.Message);
            }

            return (T)variables[name];
        }

        /// <summary>
        /// Same as initialized, but returns the value precasted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Initialize<T>(string name, Func<T> value)
        {
            if (!HasVariable(name))
            {
                variables[name] = value.Invoke();
            }

            try
            {
                _ = (T)variables[name];
                return (T)variables[name];
            }
            catch(InvalidCastException e)
            {
                Debug.LogError("Variable " + name + " could not be retrieved as type " + typeof(T).FullName + "\n Full error:" + e.Message);
            }

            return default;
        }
        #endregion
    }

    /// <summary>
    /// Methods that involve unity objects and functions
    /// </summary>
    public struct UnityUtils
    {
        /// <summary>
        /// Spawn a GameObject at a specified position
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <returns>The newly created GameObject</returns>
        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            GameObject newObj = Object.Instantiate(prefab);
            newObj.transform.position = position;
            return newObj;
        }

        /// <summary>
        /// Returns a list of objects with a given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>A list of objects with a certain tag</returns>
        public static List<GameObject> GetObjectsByTag(string tag) => GameObject.FindGameObjectsWithTag(tag).ToList();

        /// <summary>
        /// Concats a collection of objects onto a single string, seperated by a comma.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <returns>A string containing all objects in objs</returns>
        public static string Stringify<T>(IEnumerable<T> objs)
        {
            string masterString = "";
            foreach (T obj in objs)
                masterString += obj.ToString() + ",";

            return masterString;
        }

        /// <summary>
        /// Determines if an object is outside of the bounds
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the object is not in the predefined boundary</returns>
        public static bool IsOutBounds(GameObject obj) => IsOutBounds(obj.transform.position);

        /// <summary>
        /// Determines if a vector is out of bounds
        /// </summary>
        /// <param name="v"></param>
        /// <returns>True if the vector is not in the predefined bounds</returns>
        public static bool IsOutBounds(Vector3 v) => v.x < Variables.Get<Bounds>("bounds").min.x
                                                      || v.x > Variables.Get<Bounds>("bounds").max.x
                                                      || v.z < Variables.Get<Bounds>("bounds").min.z
                                                      || v.z > Variables.Get<Bounds>("bounds").max.z;
    }

    /// <summary>
    /// Methods for detecting collisions
    /// </summary>
    public struct Collision
    {
        #region AABB
        /// <summary>
        /// Tests of a collision using AABB
        /// </summary>
        /// <param name="bounds1"></param>
        /// <param name="bounds2"></param>
        /// <returns>True if there's a collision, false otherwise</returns>
        public static bool AABB(Bounds bounds1, Bounds bounds2) => bounds1.max.x > bounds2.min.x 
                                                                && bounds1.min.x < bounds2.max.x 
                                                                && bounds1.max.z > bounds2.min.z 
                                                                && bounds1.min.z < bounds2.max.z;

        /// <summary>
        /// Tests a collision between two GameObjects using AABB
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>True if the GameObjects are colliding, false otherwise</returns>
        //public static bool AABB(GameObject obj1, GameObject obj2) => AABB(obj1.GetComponent<ObjectInfo>().Bounds, obj2.GetComponent<ObjectInfo>().Bounds);
        #endregion

        #region Bounding Circle
        /// <summary>
        /// Tests for a collision between two GameObjects using Bounding Circle
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>True if there's a collision, false otherwise</returns>
        //public static bool BoundingCircle(GameObject obj1, GameObject obj2) => BoundingCircle(obj1.transform.position, obj1.GetComponent<ObjectInfo>().Radius, obj2.transform.position, obj2.GetComponent<ObjectInfo>().Radius);

        /// <summary>
        /// Tests of a collision between two circles (each defined by a point and a radius) using Bounding Circle
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="rad1"></param>
        /// <param name="pos2"></param>
        /// <param name="rad2"></param>
        /// <returns>True if the cirlces overlap, false otherwise</returns>
        public static bool BoundingCircle(Vector3 pos1, float rad1, Vector3 pos2, float rad2) => VectorUtils.CalcSqDistance(pos1, pos2) < Mathf.Pow(rad1 + rad2, 2);
        #endregion
    }
    
    /// <summary>
    /// Methods that extend on other classes
    /// </summary>
    public static class Extensions
    {
        #region Vector
        /// <summary>
        /// Returns a vector who points in the same direction and has a magnitude of scalar
        /// </summary>
        /// <param name="v"></param>
        /// <param name="scalar"></param>
        /// <returns>The scaled vector</returns>
        public static Vector3 Scale(this Vector3 v, float scalar) => v.normalized * scalar;
        
        /// <summary>
        /// Returns the same vector without it's y value
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A vector with only x and z values</returns>
        public static Vector3 StripY(this Vector3 v) => new Vector3(v.x, 0, v.z);

        public static Vector3 StripZ(this Vector3 v) => new Vector3(v.x, v.y, 0);

        /// <summary>
        /// Strips a vector of specified dimensions
        /// </summary>
        /// <param name="v"></param>
        /// <param name="dims"></param>
        /// <returns>A vector with specifed dimensions set to zero</returns>
        public static Vector3 Strip(this Vector3 v, params Dimension[] dims)
        {
            Stack<Dimension> dimStack = new Stack<Dimension>(dims);
            while(dimStack.Count > 0)
            {
                switch (dimStack.Pop())
                {
                    case Dimension.X:
                        v = new Vector3(0, v.y, v.z);
                        break;
                    case Dimension.Y:
                        v = new Vector3(v.x, 0, v.z);
                        break;
                    case Dimension.Z:
                        v = new Vector3(v.x, v.y, 0);
                        break;
                }
            }

            return v;
        }

        public static Vector2 StripZDim(this Vector3 v) => new Vector2(v.x, v.y);

        public static Vector2 StripDim(this Vector3 v, Dimension dim)
        {
            Vector2 v2;
            switch (dim)
            {
                case Dimension.X:
                    v2 = new Vector2(v.y, v.z);
                    break;
                case Dimension.Y:
                    v2 = new Vector2(v.x, v.z);
                    break;
                default:
                    v2 = new Vector2(v.x, v.y);
                    break;
            }
            return v2;
        }

        public static Vector3 FillZDim(this Vector2 v) => new Vector3(v.x, v.y, 0);

        public static Vector3 FillDim(this Vector2 v, Dimension dim, float value = 0)
        {
            Vector3 v2;
            switch (dim)
            {
                case Dimension.X:
                    v2 = new Vector3(value, v.x, v.y);
                    break;
                case Dimension.Y:
                    v2 = new Vector3(v.x, value, v.y);
                    break;
                default:
                    v2 = new Vector3(v.x, v.y, value);
                    break;
            }
            return v2;
        }

        public static Vector2 RemoveZDim(this Vector3 v) => new Vector2(v.x, v.y);

        public static Vector2 RemoveDim(this Vector3 v, Dimension dim)
        {
            Vector2 v2;
            switch (dim)
            {
                case Dimension.X:
                    v2 = new Vector2(v.y, v.z);
                    break;
                case Dimension.Y:
                    v2 = new Vector2(v.x, v.z);
                    break;
                default:
                    v2 = new Vector2(v.x, v.y);
                    break;
            }
            return v2;
        }

        /// <summary>
        /// Replace the X and Y values with the respective values from v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 ReplaceXY(this Vector3 v1, Vector3 v2) => new Vector3(v2.x, v2.y, v1.z);

        /// <summary>
        /// Replaces the values of specified dimensions with respective values from another vector
        /// </summary>
        /// <param name="v1">The original vector</param>
        /// <param name="v2">The vector to reference</param>
        /// <param name="dims">The dimensions to replace</param>
        /// <returns></returns>
        public static Vector3 Replace(this Vector3 v1, Vector3 v2, params Dimension[] dims)
        {
            Stack<Dimension> dimStack = new Stack<Dimension>(dims);
            while(dimStack.Count > 0)
            {
                switch (dimStack.Pop())
                {
                    case Dimension.X:
                        v1 = new Vector3(v2.x, v1.y, v1.z);
                        break;
                    case Dimension.Y:
                        v1 = new Vector3(v1.x, v2.y, v1.z);
                        break;
                    case Dimension.Z:
                        v1 = new Vector3(v1.x, v1.y, v2.z);
                        break;
                }
            }
            return v1;
        }

        public static Vector3 Replace(this Vector3 v1, float value, Dimension dim)
        {
            switch (dim)
            {
                case Dimension.X:
                    v1 = new Vector3(value, v1.y, v1.z);
                    break;
                case Dimension.Y:
                    v1 = new Vector3(v1.x, value, v1.z);
                    break;
                case Dimension.Z:
                    v1 = new Vector3(v1.x, v1.y, value);
                    break;
            }
            return v1;
        }

        /// <summary>
        /// Reflects the specified dimensions of the vector
        /// </summary>
        /// <param name="v"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        public static Vector3 Reflect(this Vector3 v, params Dimension[] dims)
        {
            Stack<Dimension> dimStack = new Stack<Dimension>(dims);
            while(dimStack.Count > 0)
            {
                switch (dimStack.Pop())
                {
                    case Dimension.X:
                        v = new Vector3(-v.x, v.y, v.z);
                        break;
                    case Dimension.Y:
                        v = new Vector3(v.x, -v.y, v.z);
                        break;
                    case Dimension.Z:
                        v = new Vector3(v.x, v.y, -v.z);
                        break;
                }
            }

            return v;
        }

        /// <summary>
        /// Sums up all vectors in the collection
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Sum(this IEnumerable<Vector3> v) => v.Aggregate((v1, v2) => v1 + v2);

        /// <summary>
        /// Calculates the average of all vectors in the collection
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Average(this IEnumerable<Vector3> v) => v.Sum() / v.Count();

        public static Vector3 ClampMagnitudeXZ(this Vector3 v, float mag)
        {
            float vy = v.y;
            v = Vector3.ClampMagnitude(v, mag);
            v.y = vy;
            return v;
        }
        #endregion

        #region LinkedList
        public static LinkedListNode<T> ElementAt<T>(this LinkedList<T> list, int index)
        {
            LinkedListNode<T> currentNode = list.First;
            for(int i = 0; i < index; i++)
            {
                currentNode = currentNode.Next ?? throw new IndexOutOfRangeException();
            }
            return currentNode;
        }
        #endregion
    }

    public static class Math
    {
        public static int Map(int value, int fromLow, int fromHigh, int toLow, int toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        public static float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        public static bool InRange(float value, float min, float max) => !(!(value <= min) || !(value >= max));
    }
}
