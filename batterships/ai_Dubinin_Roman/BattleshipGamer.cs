using System;
using System.Collections.Generic;
using System.Linq;
using battleships;

namespace ai_Dubinin_Roman
{
	class BattleshipGamer
	{
		private readonly GamerMap Map;
		private readonly Random Random;

		public BattleshipGamer(int mapWidth, int mapHeight)
		{
			Map = new GamerMap(mapWidth, mapHeight);
			Random = new Random();
		}

		public Vector GetNextCell()
		{
			return ChoiseNextEmptyCell();
		}

		public Vector GetNextCell(Vector lastCell, ShotEffect lastEffect)
		{
			if(lastEffect == ShotEffect.Miss)
				Map[lastCell] = CellState.Miss;
			if(lastEffect == ShotEffect.Wound)
				Map[lastCell] = CellState.Wounded;
			if (lastEffect == ShotEffect.Kill)
				LeadRoundDeadShip(lastCell);

			var edgeWoundedCells = Vector.Rect(0, 0, Map.MapWidth, Map.MapHeight)
				.Where(cell => Map[cell] == CellState.Wounded)
				.Where(cell => Map.Neighbours(cell).Count(CellIsWounded) <= 1);

			if (!edgeWoundedCells.Any())
				return ChoiseNextEmptyCell();

			var potentialCells = edgeWoundedCells
				.Where(cell => WoundedNeighbours(cell).Any())
				.Select(cell => cell.Add(DifferenceVector(cell, WoundedNeighbour(cell))))
				.Where(Map.InMapBounds)
				.Where(cell => Map[cell] == CellState.Eempty);

			if (!potentialCells.Any())
			{
				var a = Map.Neighbours(edgeWoundedCells.First())
					.Where(neighbour => Map[neighbour] == CellState.Eempty);
				return a.First();
			}

			return potentialCells.First();
		}

		public IEnumerable<Vector> Diagonals(Vector cell)
		{
			return
				new[] { new Vector(-1, -1), new Vector(-1, 1), new Vector(1, 1), new Vector(1, -1) }
				.Select(cell.Add)
				.Where(Map.InMapBounds);
		}

		private void LeadRoundDeadShip(Vector cell)
		{
			Map[cell] = CellState.Dead;
			foreach (var emptyNeighbour in EmptyNeighbours(cell))
				Map[emptyNeighbour] = CellState.Miss;

			foreach (var DiagonalNeighbour in Diagonals(cell))
			{
				if(Map[DiagonalNeighbour] == CellState.Eempty)
					Map[DiagonalNeighbour] = CellState.Miss;
			}

			foreach (var woundedNeighbour in WoundedNeighbours(cell))
				LeadRoundDeadShip(woundedNeighbour);
		}

		private Vector WoundedNeighbour(Vector cell)
		{
			return WoundedNeighbours(cell).First();
		}

		private IEnumerable<Vector> EmptyNeighbours(Vector cell)
		{
			return Map.Neighbours(cell).Where(neighbour => Map[neighbour] == CellState.Eempty);
		}

		private Vector ChoiseNextEmptyCell()
		{
			var nextCell = new Vector(Random.Next(Map.MapWidth), Random.Next(Map.MapHeight));
			while (Map[nextCell] != CellState.Eempty)
			{
				nextCell = new Vector(Random.Next(Map.MapWidth), Random.Next(Map.MapHeight));
			}
			return nextCell;
		}

		private IEnumerable<Vector> WoundedNeighbours(Vector cell)
		{
			return Map.Neighbours(cell).Where(neighbour => CellIsWounded(neighbour));
		}

		private Vector DifferenceVector(Vector firstVector, Vector secondVector)
		{
			return firstVector.Sub(secondVector);
		}

		private bool CellIsWounded(Vector neighbour)
		{
			return Map[neighbour] == CellState.Wounded;
		}
	}
}
