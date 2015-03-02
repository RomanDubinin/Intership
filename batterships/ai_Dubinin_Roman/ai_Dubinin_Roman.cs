using System;
using System.Collections.Generic;
using System.Linq;

namespace ai_Dubinin_Roman
{
	enum CellState
	{
		Eempty,
		Miss,
		Wounded,
		Dead
	}

	public enum ShotEffect
	{
		Miss,
		Wound,
		Kill
	}

	public class Vector
	{
		public Vector(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		public override string ToString()
		{
			return string.Format("({0}, {1})", X, Y);
		}

		public Vector Mult(int k)
		{
			return new Vector(k * X, k * Y);
		}

		public Vector Add(Vector v)
		{
			return new Vector(v.X + X, v.Y + Y);
		}

		public Vector Sub(Vector v)
		{
			return new Vector(X - v.X, Y - v.Y);
		}

		protected bool Equals(Vector other)
		{
			return Y == other.Y && X == other.X;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Vector)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Y * 397) ^ X;
			}
		}

		public static IEnumerable<Vector> Rect(int minX, int minY, int width, int height)
		{
			return
				from x in Enumerable.Range(minX, width)
				from y in Enumerable.Range(minY, height)
				select new Vector(x, y);
		}
	}

	class GamerMap
	{
		private static CellState[,] StatesMap;
		public int MapWidth { get; private set; }
		public int MapHeight { get; private set; }

		public GamerMap(int mapWidth, int mapHeight)
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
				new[] { new Vector(0, -1), new Vector(-1, 0), new Vector(0, 1), new Vector(1, 0) }
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

	class BattleshipGamer
	{
		private readonly GamerMap Map;
		private readonly Random Random;
		private int CurrentX;
		private int CurrentY;
		private int CurrentStep;

		public BattleshipGamer(int mapWidth, int mapHeight)
		{
			Map = new GamerMap(mapWidth, mapHeight);
			Random = new Random();
			CurrentX = 0;
			CurrentY = 0;
			CurrentStep = Map.MapWidth / 5 - 1;
		}

		public Vector GetNextCell()
		{
			return ChoiseNextEmptyCell();
		}

		public Vector GetNextCell(Vector lastCell, ShotEffect lastEffect)
		{
			if (lastEffect == ShotEffect.Miss)
				Map[lastCell] = CellState.Miss;
			if (lastEffect == ShotEffect.Wound)
				Map[lastCell] = CellState.Wounded;
			if (lastEffect == ShotEffect.Kill)
				LeadRoundDeadShip(lastCell);

			var woundedCells = Vector.Rect(0, 0, Map.MapWidth, Map.MapHeight)
				.Where(CellIsWounded);

			if (!woundedCells.Any())
				return ChoiseNextEmptyCell();

			var potentialCells = woundedCells
				.SelectMany(NextTargets)
				.Where(Map.InMapBounds)
				.Where(CellIsEmpty);

			return potentialCells.First();
		}

		private IEnumerable<Vector> NextTargets(Vector cell)
		{
			if (WoundedNeighbours(cell).Any())
				return WoundedNeighbours(cell)
					.Select(neighbour => cell.Add(DifferenceVector(cell, neighbour)));

			return Map.Neighbours(cell).
				Where(CellIsEmpty);

		}


		private void LeadRoundDeadShip(Vector cell)
		{
			Map[cell] = CellState.Dead;

			foreach (var emptyNeighbour in EmptyNeighbours(cell))
				Map[emptyNeighbour] = CellState.Miss;

			foreach (var diagonalNeighbour in Map.Diagonals(cell))
				Map[diagonalNeighbour] = CellState.Miss;

			foreach (var woundedNeighbour in WoundedNeighbours(cell))
				LeadRoundDeadShip(woundedNeighbour);
		}

		private Vector ChoiseNextEmptyCell()
		{
			for (var step = CurrentStep; step > 0; step--)
			{
				for (var y = CurrentY; y < Map.MapHeight; y += 1)
				{
					for (var x = CurrentX; x < Map.MapWidth; x += step)
					{
						var currentCell = new Vector(x, y);
						if (CellIsEmpty(currentCell))
						{
							CurrentStep = step;
							CurrentX = x;
							CurrentY = y;
							return currentCell;
						}
					}
					CurrentX = step / 2 * ((y + 1) % 2);
				}

				CurrentY = 0;
			}
			throw new Exception("There are no more empty cells");
		}

		private Vector ChoiseNextEmptyCellRandom()
		{
			var nextCell = new Vector(Random.Next(Map.MapWidth), Random.Next(Map.MapHeight));

			while (Map[nextCell] != CellState.Eempty)
				nextCell = new Vector(Random.Next(Map.MapWidth), Random.Next(Map.MapHeight));

			return nextCell;
		}

		private Vector DifferenceVector(Vector firstVector, Vector secondVector)
		{
			return firstVector.Sub(secondVector);
		}

		private IEnumerable<Vector> EmptyNeighbours(Vector cell)
		{
			return Map.Neighbours(cell).Where(CellIsEmpty);
		}

		private IEnumerable<Vector> WoundedNeighbours(Vector cell)
		{
			return Map.Neighbours(cell).Where(CellIsWounded);
		}

		private bool CellIsEmpty(Vector cell)
		{
			return Map[cell] == CellState.Eempty;
		}

		private bool CellIsWounded(Vector cell)
		{
			return Map[cell] == CellState.Wounded;
		}
	}

	class ai_Dubinin_Roman
	{
		static void Main()
		{
			var gameData = GameData();
			var gamer = InitGamer(gameData);

			var nextCell = gamer.GetNextCell();
			DoShot(nextCell.X, nextCell.Y);

			gameData = GameData();
			while (gameData != null)
			{
				
				if (IsInitString(gameData))
				{
					gamer = InitGamer(gameData);
					nextCell = gamer.GetNextCell();
				}
				else
				{
					var lastEffect = ShotResult(gameData);
					var lastCell = GetCell(gameData);
					nextCell = gamer.GetNextCell(lastCell, lastEffect);
				}
				DoShot(nextCell.X, nextCell.Y);

				gameData = GameData();
			}
		}

		private static bool IsInitString(string gameData)
		{
			return String.Compare(gameData.Split(' ').First(), "Init", StringComparison.Ordinal) == 0;
		}

		private static Vector GetCell(string lastShotData)
		{
			var shotParams = lastShotData.Split(' ');
			var width = int.Parse(shotParams[1]);
			var height = int.Parse(shotParams[2]);
			return new Vector(width, height);
		}

		private static BattleshipGamer InitGamer(string lastShotData)
		{
			var gameParams = lastShotData.Split(' ');
			var width = int.Parse(gameParams[1]);
			var height = int.Parse(gameParams[2]);
			return new BattleshipGamer(width, height);
		}

		private static string GameData()
		{
			return Console.ReadLine();
		}

		private static void DoShot(int x, int y)
		{
			Console.WriteLine("{0} {1}", x, y);
		}

		private static ShotEffect ShotResult(string lastShotData)
		{
			var effestName = lastShotData.Split(' ').First();
			return (ShotEffect)Enum.Parse(typeof(ShotEffect), effestName);
		}

		
	}
}
