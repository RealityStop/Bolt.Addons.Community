using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRaycast_Children
    {
        /// <summary>
        /// Begins a raycast with multiple of something.
        /// </summary>
        public static HUMRaycast.Data.Multiple Multiple(this HUMRaycast.Data.With withData)
        {
            return new HUMRaycast.Data.Multiple(withData.origin);
        }

        /// <summary>
        /// Begins finding something that has some other specific data.
        /// </summary>
        public static HUMRaycast.Data.With With(this HUMRaycast.Data.Find findData)
        {
            return new HUMRaycast.Data.With(findData.origin);
        }

        /// <summary>
        /// Begins checking if a ray is touching something.
        /// </summary>
        public static HUMRaycast.Data.Touching Touching(this HUMRaycast.Data.Is isData)
        {
            return new HUMRaycast.Data.Touching(isData.origin);
        }

        /// <summary>
        /// Returns a contacted GameObject if we have touched an object using a specific ray length, and got back an object with the tag "Environment".
        /// </summary>
        public static ContactConnection<GameObject> Environment(this HUMRaycast.Data.Touching touchData, Vector3 direction, float length)
        {
            RaycastHit[] hit = Physics.RaycastAll(touchData.origin, direction, length);

            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider.tag == "Environment")
                {
                    return new ContactConnection<GameObject>(hit[i].collider.gameObject);
                }
            }

            return new ContactConnection<GameObject>(null);
        }

        /// <summary>
        /// Returns a contacted GameObject if we have touched an object, and got back an object with the tag "Environment".
        /// </summary>
        public static ContactConnection<GameObject> Environment(this HUMRaycast.Data.Touching touchData, Vector3 direction)
        {
            RaycastHit[] hit = Physics.RaycastAll(touchData.origin, direction);

            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider.tag == "Environment")
                {
                    return new ContactConnection<GameObject>(hit[i].collider.gameObject);
                }
            }

            return new ContactConnection<GameObject>(null);
        }

        /// <summary>
        /// Returns a contacted GameObject if we have touched the bottom of something, or a "Ceiling".
        /// </summary>
        public static ContactConnection<GameObject> Ceiling(this HUMRaycast.Data.Touching touchData, float length)
        {
            Vector3 origin = new Vector3(
                touchData.origin.x,
                touchData.origin.y,
                touchData.origin.z
                );

            Debug.DrawRay(origin, Vector3.up, Color.red);
            return Environment(touchData, Vector3.up, length);
        }

        /// <summary>
        /// Returns a contacted GameObject if we have touched a floor using a specific ray length.
        /// </summary>
        public static ContactConnection<GameObject> Floor(this HUMRaycast.Data.Touching touchData, float length)
        {
            Vector3 origin = new Vector3(
                touchData.origin.x,
                touchData.origin.y,
                touchData.origin.z
                );
            Debug.DrawRay(origin, Vector3.down, Color.red);
            return Environment(touchData, Vector3.down, length);
        }

        /// <summary>
        /// Performs a raycast operation in a 2D grid like fashion. Choose rows and columns, the length, and its space. Return the ContactConnection. You must supply a forward and up vector to determine direction of the grid.
        /// </summary>
        public static ContactConnection<T> Rays<T>(this HUMRaycast.Data.Multiple multipleData, Vector3 forward, Vector3 up, Vector2 spacing, int rows, int columns, float rayLength, out RaycastHit[] hit)
            where T : class
        {
            T contact = null;
            hit = null;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector3 castOrigin = multipleData.origin;

                    castOrigin += Vector3.Cross(up.normalized, forward.normalized).Multiply(row) * spacing.y;
                    castOrigin += -up.Multiply(column) * spacing.x;

                    hit = Physics.RaycastAll(castOrigin, forward, rayLength);

                    Debug.DrawRay(castOrigin, forward * rayLength, Color.red);

                    for (int k = 0; k < hit.Length; k++)
                    {
                        contact = hit[k].collider.GetComponent<T>();

                        if (contact != null)
                        {
                            var connection = new ContactConnection<T>(contact, hit[k].distance);
                            return connection;
                        }
                    }
                }
            }

            return new ContactConnection<T>(null);
        }

        /// <summary>
        /// Performs a raycast operation in a 2D grid like fashion. Choose rows and columns, the length, and its space. Return the ContactConnection. You must supply a transform to know the direction from the origin.
        /// </summary>
        public static ContactConnection<T> Rays<T>(this HUMRaycast.Data.Multiple multipleData, Transform targetPoint, Vector2 spacing, int rows, int columns, float rayLength, out RaycastHit[] hit)
            where T : class
        {
            T contact = null;
            hit = null;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector3 castOrigin = multipleData.origin;

                    castOrigin += -targetPoint.right.Multiply(row) * spacing.y;
                    castOrigin += -targetPoint.up.Multiply(column) * spacing.x;

                    hit = Physics.RaycastAll(castOrigin, targetPoint.forward, rayLength);

                    Debug.DrawRay(castOrigin, targetPoint.forward * rayLength, Color.red);

                    for (int k = 0; k < hit.Length; k++)
                    {
                        contact = hit[k].collider.GetComponent<T>();

                        if (contact != null)
                        {
                            var connection = new ContactConnection<T>(contact, hit[k].distance);
                            return connection;
                        }
                    }
                }
            }

            return new ContactConnection<T>(null);
        }
    }
}