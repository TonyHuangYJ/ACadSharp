﻿using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellValue
		{
			public CellValueType ValueType { get; set; }

			public int Units { get; set; }

			public int Flags { get; set; }

			public bool IsEmpty
			{
				get
				{
					return (this.Flags & 1) != 0;
				}
				set
				{
					if (value)
					{
						this.Flags |= 0b1;
					}
					else
					{
						this.Flags &= ~0b1;
					}
				}
			}

			/// <summary>
			/// Text string in a cell.
			/// </summary>
			[DxfCodeValue(1)]
			public string Text { get; set; }

			[DxfCodeValue(300)]
			public string Format { get; set; }

			[DxfCodeValue(302)]
			public string FormatedValue { get; set; }

			public object Value { get; set; }

			public override string ToString()
			{
				return this.Value.ToString();
			}
		}
	}
}
