using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Structures
{
    /// <summary>
    /// Contains a position.
    /// Note: Struct really is faster then class so use struct or class depending on what you need
    /// </summary>
    public class MazePointClassLinkedList : MazePointClass
    {
        public MazePointClassLinkedList Previous { get; set; }
        public MazePointClassLinkedList Next { get; set; }

        public MazePointClassLinkedList(int x, int y)
            : base(x, y)
        {
        }

        public IEnumerable<MazePointClassLinkedList> AsEnumerable()
        {
            var cur = this;

            while (cur != null)
            {
                yield return cur;
                cur = cur.Next;
            }
        }

        public void InsertMeInBetweenTheseTwo(MazePointClassLinkedList previous, MazePointClassLinkedList next)
        {
            Previous = previous;
            Next = next;

            previous.Next = this;
            next.Previous = this;
        }
    }
}