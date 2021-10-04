using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMTransform_Children
    {
        /// <summary>
        /// Snaps a transforms center.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Center(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Center, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms bottom.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Bottom(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Bottom, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms top.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Top(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Top, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms left.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Left(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Left, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms right.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Right(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Right, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms front.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Front(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Front, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms back.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Back(this HUMTransform.Data.From from)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Back, Vector3.zero);
        }

        /// <summary>
        /// Snaps a transforms center with an offset.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Center(this HUMTransform.Data.From from, Vector3 offset)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Center, offset);
        }

        /// <summary>
        /// Snaps a transforms bottom with an offset.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Bottom(this HUMTransform.Data.From from, Vector3 offset)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Bottom, offset);
        }

        /// <summary>
        /// Snaps a transforms top with an offset.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Top(this HUMTransform.Data.From from, Vector3 offset)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Top, offset);
        }

        /// <summary>
        /// Snaps a transforms left with an offset.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Left(this HUMTransform.Data.From from, Vector3 offset)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Left, offset);
        }

        /// <summary>
        /// Snaps a transforms right with an offset.
        /// </summary>
        public static HUMTransform.Data.SnapPosition Right(this HUMTransform.Data.From from, Vector3 offset)
        {
            return new HUMTransform.Data.SnapPosition(from, HUMTransform.Data.TransformSnapPosition.Right, offset);
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms top surface.
        /// </summary>
        public static Vector3 Top(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Top().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms bottom bounding box bounds.
        /// </summary>
        public static Vector3 Bottom(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Top().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, bounds.snapTo.localScale.y / 2, 0));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms left bounding box bounds.
        /// </summary>
        public static Vector3 Left(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position - (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms right bounding box bounds.
        /// </summary>
        public static Vector3 Right(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position + (new Vector3(bounds.snapTo.localScale.x / 2, 0, 0));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms front bounding box bounds.
        /// </summary>
        public static Vector3 Front(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position + (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Snaps a previous transforms specified point to another transforms back bounding box bounds.
        /// </summary>
        public static Vector3 Back(this HUMTransform.Data.ObjectBounds bounds, float padding = 0f)
        {
            var pointOffset = Vector3.zero;

            switch (bounds.to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Top:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Center().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Bottom().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Left().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Right().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Front:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Front().To().Point(pointOffset, padding);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Back:
                    pointOffset = bounds.snapTo.position - (new Vector3(0, 0, bounds.snapTo.localScale.z / 2));
                    bounds.to.positionData.from.transform.Snap().Back().To().Point(pointOffset, padding);
                    break;
            }

            return pointOffset;
        }

        /// <summary>
        /// Returns a transforms size split in half.
        /// </summary>
        public static Vector3 Half(this HUMTransform.Data.Size data)
        {
            var localScale = data.transform == null ? Vector3.zero : data.transform.localScale / 2;
            return localScale;
        }

        /// <summary>
        /// Moves a transform backwards at a specified speed.
        /// </summary>
        public static void Backward(this HUMTransform.Data.Move moveData, float speed, TimeType timeType = TimeType.Delta)
        {
            moveData.transform.position -= moveData.transform.forward * (speed * timeType.GetTime());
        }

        /// <summary>
        /// Moves a transform left at a specified speed.
        /// </summary>
        public static void Left(this HUMTransform.Data.Move moveData, float speed, TimeType timeType = TimeType.Delta)
        {

            moveData.transform.position -= moveData.transform.right * (speed * timeType.GetTime());
        }

        /// <summary>
        /// Moves a transform right at a specific speed.
        /// </summary>
        public static void Right(this HUMTransform.Data.Move moveData, float speed, TimeType timeType = TimeType.Delta)
        {
            moveData.transform.position += moveData.transform.right * (speed * timeType.GetTime());
        }

        /// <summary>
        /// Continues a transform looking in some way.
        /// </summary>
        public static HUMTransform.Data.In In(this HUMTransform.Data.Look lookData)
        {
            return new HUMTransform.Data.In(lookData);
        }

        /// <summary>
        /// Continues a transform looking at something.
        /// </summary>
        public static HUMTransform.Data.At At(this HUMTransform.Data.Look lookData)
        {
            return new HUMTransform.Data.At(lookData);
        }

        /// <summary>
        /// Sets an objects position at point with custom origin snapping.
        /// </summary>
        public static Vector3 Point(this HUMTransform.Data.To to, Vector3 point, float padding = 0f)
        {
            var snapObjectHalf = to.positionData.from.transform.Size().Half();

            switch (to.positionData.position)
            {
                case HUMTransform.Data.TransformSnapPosition.Center:
                    to.positionData.from.transform.position = point;
                    break;
                case HUMTransform.Data.TransformSnapPosition.Top:
                    to.positionData.from.transform.position = new Vector3(point.x, point.y - padding - snapObjectHalf.y, point.z);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Bottom:
                    to.positionData.from.transform.position = new Vector3(point.x, point.y + padding + snapObjectHalf.y, point.z);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Left:
                    to.positionData.from.transform.position = new Vector3(point.x + padding + snapObjectHalf.x, point.y, point.z);
                    break;

                case HUMTransform.Data.TransformSnapPosition.Right:
                    to.positionData.from.transform.position = new Vector3(point.x - padding - snapObjectHalf.x, point.y, point.z);
                    break;
            }

            return point;
        }

        /// <summary>
        /// Sets a transforms position to a point in space, but locks an axis from being altered.
        /// </summary>
        public static Vector2 Point(this HUMTransform.Data.To to, Vector2 point, Axis lockedAxis)
        {
            switch (lockedAxis)
            {
                case Axis.X:
                    return Point(to, new Vector3(to.positionData.from.transform.position.x, point.x, point.y));

                case Axis.Y:
                    return Point(to, new Vector3(point.x, to.positionData.from.transform.position.y, point.y));

                case Axis.Z:
                    return Point(to, new Vector3(point.x, point.y, to.positionData.from.transform.position.z));
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Starts the operation of snapping to the outside of a bounding box which is created from the center of a transform.
        /// </summary>
        public static HUMTransform.Data.ObjectBounds ObjectBounds(this HUMTransform.Data.To to, Transform snapTo)
        {
            var snapObjectHalf = to.positionData.from.transform.Size().Half();
            var snapToHalf = snapTo.Size().Half();
            var snapSurfaceData = new HUMTransform.Data.ObjectBounds(snapTo, to, snapObjectHalf, snapToHalf);
            return snapSurfaceData;
        }

        /// <summary>
        /// Moves a transform by speed, in the forward direction.
        /// </summary>
        public static void Forward(this HUMTransform.Data.Move moveData, float speed, TimeType timeType = TimeType.Delta)
        {
            moveData.transform.position += moveData.transform.forward * (speed * timeType.GetTime());
        }

        /// <summary>
        /// Moves a transform by speed, in the forward direction.
        /// </summary>
        public static void Forward(this HUMTransform.Data.MoveRigidbody moveData, Transform transform, float speed, TimeType timeType = TimeType.Delta)
        {
            moveData.rigidbody.MovePosition(moveData.rigidbody.position + (transform.forward * (speed * timeType.GetTime())));
        }

        /// <summary>
        /// Begins the operation of snapping one transform to another point in space.
        /// </summary>
        public static HUMTransform.Data.To To(this HUMTransform.Data.SnapPosition positionData)
        {
            return new HUMTransform.Data.To(positionData);
        }

        /// <summary>
        /// Looks in the direction of a point in space.
        /// </summary>
        public static void Direction(this HUMTransform.Data.In inData, Vector3 direction)
        {
            inData.look.transform.forward = direction.normalized;
        }

        /// <summary>
        /// Looks at a Game Object.
        /// </summary>
        public static void Object(this HUMTransform.Data.At atData, GameObject gameObject)
        {
            atData.look.transform.forward = atData.look.transform.position - gameObject.transform.position;
        }
    }
}