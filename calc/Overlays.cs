using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static calc.Form1;

namespace calc
{
    public class Overlays : IDisposable
	{
		private readonly GraphicsWindow Window;
		private readonly Dictionary<string, SolidBrush> Brushes;
		private Font font;
		private Form1 form;

		public Overlays()
		{
			Brushes = new Dictionary<string, SolidBrush>();

			var Graphic = new Graphics();

			Window = new GraphicsWindow(Graphic)
			{
				FPS = 60,
				IsTopmost = true,
			};

			Window.DestroyGraphics += Window_DestroyGraphics;
			Window.DrawGraphics += Window_DrawGraphics;
			Window.SetupGraphics += Window_SetupGraphics;
		}

		private void Window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
		{
			var Graphic = e.Graphics;

			Window.FitTo(Memory.m_iProcess.MainWindowHandle);
			Brushes["enemy"] = Graphic.CreateSolidBrush(255, 128, 128, 0.7f);
			Brushes["teammate"] = Graphic.CreateSolidBrush(128, 255, 128, 0.7f);

			font = Graphic.CreateFont("Arial", 64);

			Window.FitTo(Memory.m_iProcess.MainWindowHandle);
		}

		private void Window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
		{
			foreach (var Brush in Brushes) Brush.Value.Dispose();
		}

		private void Window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
		{
			Graphics gfx = e.Graphics;
			gfx.ClearScene();
			gfx.BeginScene();
			WallHack(gfx);

			if (b_KeyboardDown)
				gfx.DrawText(font, Brushes["teammate"], 0, Window.Height / 2, $"Aimbot: On");
			else
				gfx.DrawText(font, Brushes["enemy"], 0, Window.Height / 2, $"Aimbot: Off");

			gfx.EndScene();
		}

		private unsafe void WallHack(Graphics graphics)
		{
			Vector3 vTemp1;
			Vector3 vTemp2;

			fixed (float* matrix = BigHack.ViewMatrix.m)
			{
				foreach (var player in BigHack.players)
				{
					if (!player.IsAlive()) continue;

					foreach ((int A, int B) line in player.SkeletonIdx)
					{
						vTemp1 = player.SkeletonPos[line.A];
						vTemp2 = player.SkeletonPos[line.B];

						if (WorldToScreen(matrix, Window.Height, Window.Width, ref vTemp1) && WorldToScreen(matrix, Window.Height, Window.Width, ref vTemp2))
						{
							if (player.Team == BigHack.player.Team)
								graphics.DrawLine(Brushes["teammate"], vTemp1.X, vTemp1.Y, vTemp2.X, vTemp2.Y, 3f);
							else
								graphics.DrawLine(Brushes["enemy"], vTemp1.X, vTemp1.Y, vTemp2.X, vTemp2.Y, 3f);
						}
					}
				}
			}
		}

		private unsafe bool WorldToScreen(float* viewMatrix, int height, int width, ref Vector3 pos)
		{
			float screenX = viewMatrix[0] * pos.X + viewMatrix[1] * pos.Y + viewMatrix[2] * pos.Z + viewMatrix[3];
			float screenY = viewMatrix[4] * pos.X + viewMatrix[5] * pos.Y + viewMatrix[6] * pos.Z + viewMatrix[7];
			float screenW = viewMatrix[12] * pos.X + viewMatrix[13] * pos.Y + viewMatrix[14] * pos.Z + viewMatrix[15];

			if (!(screenW > 0)) return false; //Basicly behind us

			pos.X = (1 + screenX / screenW) * width / 2 + 0.5f;
			pos.Y = (1 - screenY / screenW) * height / 2 + 0.5f;

			if (pos.X < 0 || pos.X > width || pos.Y < 0 || pos.Y > height) return false;

			return true;
		}

		public void Run()
		{
			Window.Create();
			Window.Join();
		}

		~Overlays()
		{
			Dispose(false);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				Window.Dispose();

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}