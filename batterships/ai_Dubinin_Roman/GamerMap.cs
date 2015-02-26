using System;
using System.Collections.Generic;
using System.Linq;
using battleships;

namespace ai_Dubinin_Roman
{
	enum CellState
	{
		Eempty,
		Miss,
		Wounded,
		Dead
	}
	class GamerMap
	{
		private static CellState[,] StatesMap;
		public int MapWidth { get; private set; }
		public int MapHeight { get; private set; }
		
		public GamerMap( int mapWidth, int mapHeight)
		{
			MapWidth = mapWidth;
			MapHeight = mapHeight;
			StatesMap = new CellState[mapWidth, mapHeight];
		}

		public CellState this[Vector p]
		{
			get
			{
				if (!InMapBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders");
				return StatesMap[p.X, p.Y];
			}
			set
			{
				if (!InMapBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders");
				StatesMap[p.X, p.Y] = value;
			}
		}

		public IEnumerable<Vector> Neighbours(Vector cell)
		{
			return
				from v in new[] { new Vector(0, -1), new Vector(-1, 0), new Vector(0, 1), new Vector(1, 0) }
				let c = cell.Add(v)
				where InMapBounds(c)
				select c;
		}

		public bool InMapBounds(Vector p)
		{
			return p.X >= 0 &&
				   p.X < MapWidth &&
				   p.Y >= 0 &&
				   p.Y < MapHeight;
		}


	}
}
