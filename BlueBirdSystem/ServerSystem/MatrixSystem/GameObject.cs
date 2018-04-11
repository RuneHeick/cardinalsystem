using MatrixSystem.ObjectMatrix.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem
{
    public delegate void GameObjectChanged(GameObject obj, UInt64 changedArray);

    public class GameObject
    {
        private static Random random = new Random();
        private int[] data = new int[5];

        public UInt32 ObjectID = GameObjectCounter.GetUniqueID;

        private PositionValue _position = new PositionValue();
        public PositionValue Position => _position;


        public void AffectObject(GameObject item)
        {
            item.data[0] += 80 * random.Next();
        }

        public void UpdateObject()
        {
            //if (ObjectID == 1)
            {
                Position.X = Position.X + ((20000 * random.NextDouble()) - (20000 / 2));
                Position.Y = Position.Y + ((20000 * random.NextDouble()) - (20000 / 2));
                GameObjectDataChanged?.Invoke(this, (1 << (int)PublicSyncTypes.POSITION));
            }
        }

        public List<byte[]> getBytes(uint mask)
        {
            if(mask != (1 << (int)PublicSyncTypes.POSITION))
                return new List<byte[]> { new byte[8 * 2 + 500] };
            return new List<byte[]> { new byte[8*2+5] };
        }

        public int SetBytes(byte[] data, int startIndex)
        {
            return startIndex;
        }

        public event GameObjectChanged GameObjectDataChanged;

    }
}
