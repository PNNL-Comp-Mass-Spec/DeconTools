using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Data.Structures
{
        public class BitArray2D
        {
            private readonly int _width;
            private readonly int _height;
            private System.Collections.BitArray _bitarray;

            public System.Collections.BitArray BitArray
            {
                get
                {
                    return this._bitarray;
                }
            }

            public int Width
            {
                get { return _width; }
            }

            public int Height
            {
                get { return _height; }
            }

            public BitArray2D(int cols, int rows)
            {
                if (cols <= 0)
                {
                    throw new System.ArgumentOutOfRangeException("cols");
                }

                if (rows <= 0)
                {
                    throw new System.ArgumentOutOfRangeException("rows");
                }

                this._width = cols;
                this._height = rows;
                this._bitarray = new System.Collections.BitArray(this._width * this._height);
            }

            public bool this[int col, int row]
            {
                get { return this.Get(col, row); }
                set { this.Set(col, row, value); }
            }

            public void Set(int col, int row, bool b)
            {
                if (col < 0)
                {
                    throw new System.ArgumentOutOfRangeException("col");
                }

                if (col >= this._width)
                {
                    throw new System.ArgumentOutOfRangeException("col");
                }

                if (row < 0)
                {
                    throw new System.ArgumentOutOfRangeException("row");
                }

                if (row >= this._height)
                {
                    throw new System.ArgumentOutOfRangeException("row");
                }

                var pos = (row * _width) + col;
                this.BitArray[pos] = b;
            }

            public bool Get(int col, int row)
            {
                if (col < 0)
                {
                    throw new System.ArgumentOutOfRangeException("col");
                }

                if (col >= this._width)
                {
                    throw new System.ArgumentOutOfRangeException("col");
                }

                if (row < 0)
                {
                    throw new System.ArgumentOutOfRangeException("row");
                }

                if (row >= this._height)
                {
                    throw new System.ArgumentOutOfRangeException("row");
                }

                var pos = (row * _width) + col;
                return this.BitArray[pos];
            }

            /// <summary>
            /// Creates a copy of the BitArray with the same values
            /// </summary>
            /// <returns></returns>
            public BitArray2D Clone()
            {
                var new_bitarray2d = new BitArray2D(this._width, this._height);

                for (var i = 0; i < this.BitArray.Length; i++)
                {
                    new_bitarray2d.BitArray[i] = this.BitArray[i];
                }

                return new_bitarray2d;
            }

            public void SetAll(bool value)
            {
                this._bitarray.SetAll(value);
            }


            public void Or(BitArray2D anotherArray)
            {

                this._bitarray.Or(anotherArray._bitarray);
            }

            public void Not()
            {
                this._bitarray.Not();
            }

            public byte[] ToBytes()
            {
                return BitArrayToBytes(this._bitarray);
            }
            public static byte[] BitArrayToBytes(System.Collections.BitArray bitarray)
            {
                if (bitarray.Length == 0)
                {
                    throw new System.ArgumentException("must have at least length 1", "bitarray");
                }

                var num_bytes = bitarray.Length / 8;

                if (bitarray.Length % 8 != 0)
                {
                    num_bytes += 1;
                }

                var bytes = new byte[num_bytes];
                bitarray.CopyTo(bytes, 0);
                return bytes;
            }
        }
    }
