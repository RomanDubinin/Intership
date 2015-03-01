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
					throw new IndexOutOfRangeException(string.Format("{0} is not in the map borders", p));
				return StatesMap[p.X, p.Y];
			}
			set
			{
				if (!InMapBounds(p))
					throw new IndexOutOfRangeException(string.Format("{0} is not in the map borders", p));
				StatesMap[p.X, p.Y] = value;
			}
		}

		public IEnumerable<Vector> Neighbours(Vector cell)
		{
			return 
				new[] {new Vector(0, -1), new Vector(-1, 0), new Vector(0, 1), new Vector(1, 0)}
				.Select(cell.Add)
				.Where(InMapBounds);
		}

		public IEnumerable<Vector> Diagonals(Vector cell)
		{
			return
				new[] { new Vector(-1, -1), new Vector(-1, 1), new Vector(1, 1), new Vector(1, -1) }
				.Select(cell.Add)
				.Where(InMapBounds);
		}

		public bool InMapBounds(Vector cell)
		{
			return cell.X >= 0 &&
				   cell.X < MapWidth &&
				   cell.Y >= 0 &&
				   cell.Y < MapHeight;
		}


	}
}
