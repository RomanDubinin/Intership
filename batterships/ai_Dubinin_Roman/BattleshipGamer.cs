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
				.Where(cell => Map.Neighbours(cell).Count(CellIsWounded) == 1);

			if (!edgeWoundedCells.Any())
				return ChoiseNextEmptyCell();

			var potentialCells = edgeWoundedCells
				.Select(cell => cell.Sub(DifferenceVector(cell, WoundedNeighbour(cell))));

			if (!potentialCells.Any())
				return ChoiseNextEmptyCell();

			return potentialCells.First();
		}

		private void LeadRoundDeadShip(Vector cell)
		{
			Map[cell] = CellState.Dead;
			foreach (var emptyNeighbour in EmptyNeighbours(cell))
				Map[emptyNeighbour] = CellState.Miss;
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
			return new Vector(Random.Next(Map.MapWidth), Random.Next(Map.MapHeight));
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
