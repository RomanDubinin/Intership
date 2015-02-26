using System;
using System.Linq;
using battleships;

namespace ai_Dubinin_Roman
{
	class Program
	{
		// line имеет один из следующих форматов:
		// Init <map_width> <map_height> <ship1_size> <ship2_size> ...
		// Wound <last_shot_X> <last_shot_Y>
		// Kill <last_shot_X> <last_shot_Y>
		// Miss <last_shot_X> <last_shot_Y>
		// Один экземпляр вашей программы может быть использван для проведения нескольких игр подряд.
		// Сообщение Init сигнализирует о том, что началась новая игра.

		static void Main()
		{
			
			//var gamer = new BattleshipGamer()
		}

		private string GetGameData()
		{
			return Console.ReadLine();
		}

		private void DoShot(int x, int y)
		{
			Console.WriteLine("{0} {1}", x, y);
		}

		private ShotEffect ShotResult(string data)
		{
			var effestName = data.Split(' ').First();
			return (ShotEffect)Enum.Parse(typeof(ShotEffect), effestName);
		}

		
	}
}
