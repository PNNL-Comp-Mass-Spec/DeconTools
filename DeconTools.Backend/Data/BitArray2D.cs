
namespace DeconTools.Backend.Data.Structures
{
        public class BitArray2D
        {
            public System.Collections.BitArray BitArray { get; }

            public int Width { get; }

            public int Height { get; }

            public BitArray2D(int cols, int rows)
            {
                if (cols <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(cols));
                }

                if (rows <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(rows));
                }

                Width = cols;
                Height = rows;
                BitArray = new System.Collections.BitArray(Width * Height);
            }

            public bool this[int col, int row]
            {
                get => Get(col, row);
                set => Set(col, row, value);
            }

            public void Set(int col, int row, bool b)
            {
                if (col < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(col));
                }

                if (col >= Width)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(col));
                }

                if (row < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(row));
                }

                if (row >= Height)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(row));
                }

                var pos = (row * Width) + col;
                BitArray[pos] = b;
            }

            public bool Get(int col, int row)
            {
                if (col < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(col));
                }

                if (col >= Width)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(col));
                }

                if (row < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(row));
                }

                if (row >= Height)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(row));
                }

                var pos = (row * Width) + col;
                return BitArray[pos];
            }

            /// <summary>
            /// Creates a copy of the BitArray with the same values
            /// </summary>
            /// <returns></returns>
            public BitArray2D Clone()
            {
                var new2DArray = new BitArray2D(Width, Height);

                for (var i = 0; i < BitArray.Length; i++)
                {
                    new2DArray.BitArray[i] = BitArray[i];
                }

                return new2DArray;
            }

            public void SetAll(bool value)
            {
                BitArray.SetAll(value);
            }


            public void Or(BitArray2D anotherArray)
            {

                BitArray.Or(anotherArray.BitArray);
            }

            public void Not()
            {
                BitArray.Not();
            }

            //public byte[] ToBytes()
            //{
            //    return BitArrayToBytes(BitArray);
            //}

            //public static byte[] BitArrayToBytes(System.Collections.BitArray bitArray)
            //{
            //    if (bitArray.Length == 0)
            //    {
            //        throw new System.ArgumentException("must have at least length 1", nameof(bitArray));
            //    }

            //    var num_bytes = bitArray.Length / 8;

            //    if (bitArray.Length % 8 != 0)
            //    {
            //        num_bytes += 1;
            //    }

            //    var bytes = new byte[num_bytes];
            //    bitArray.CopyTo(bytes, 0);
            //    return bytes;
            //}
        }
    }
