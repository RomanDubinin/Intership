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
		private int CurrentX;
		private int CurrentY;
		private int CurrentStep;

		public BattleshipGamer(int mapWidth, int mapHeight)
		{
			Map = new GamerMap(mapWidth, mapHeight);
			Random = new Random();
			CurrentX = 0;
			CurrentY = 0;
			CurrentStep = Map.MapWidth/5 - 1;
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
					CurrentX = step/2*((y+1) % 2);
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
}
