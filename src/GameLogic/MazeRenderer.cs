using System.Drawing;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    // draws the maze using state so Paint stays small
    internal class MazeRenderer
    {
        // shared state so draw can reuse paths, tokens etc.
        private readonly ChallengeMazeState state;
        private bool[,] labExplored;
        private bool[,] labPath;
        private bool showFrontier;
        private bool hideTokens;

        public MazeRenderer(ChallengeMazeState MazeFormState)
        {
            state = MazeFormState;
        }

        // used by lab sandbox to show overlays and hide tokens
        public void SetLabOverlays(bool[,] explored, bool[,] onPath, bool highlightFrontier, bool skipTokens)
        {
            labExplored = explored;
            labPath = onPath;
            showFrontier = highlightFrontier;
            hideTokens = skipTokens;
        }

        public void Draw(PaintEventArgs e, Control surface)
        {
            if (state.Maze == null)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.Clear(state.MazeBaseColor);

            int cellW = System.Math.Max(1, surface.Width / state.Maze.width);
            int cellH = System.Math.Max(1, surface.Height / state.Maze.height);

            DrawPaths(g, cellW, cellH);
            DrawLabOverlays(g, cellW, cellH);
            if (!hideTokens)
            {
                DrawTokens(g, cellW, cellH);
            }
            DrawExit(g, cellW, cellH);
            DrawMonster(g, cellW, cellH);
            DrawRunner(g, cellW, cellH);
        }

        private void DrawPaths(Graphics g, int cellW, int cellH)
        {
            for (int y = 0; y < state.Maze.height; y++)
            {
                for (int x = 0; x < state.Maze.width; x++)
                {
                    if (state.Maze.grid[x, y] == 1)
                    {
                        Rectangle rect = new Rectangle(
                            x * cellW,
                            y * cellH,
                            cellW,
                            cellH);
                        if (x == state.Maze.start.X && y == state.Maze.start.Y)
                        {
                            g.FillRectangle(Brushes.LightGreen, rect); // show start clearly
                        }
                        else if (x == state.Maze.exit.X && y == state.Maze.exit.Y)
                        {
                            g.FillRectangle(Brushes.IndianRed, rect); // show exit clearly
                        }
                        else
                        {
                            g.FillRectangle(Brushes.White, rect);
                        }
                    }
                    else if (state.Maze.grid[x, y] == 2 && showFrontier)
                    {
                        Rectangle rect = new Rectangle(
                            x * cellW,
                            y * cellH,
                            cellW,
                            cellH);
                        g.FillRectangle(Brushes.LightGray, rect);
                    }
                }
            }
        }

        private void DrawTokens(Graphics g, int cellW, int cellH)
        {
            if (state.Maze == null || state.Maze.tokens == null)
            {
                return;
            }

            foreach (var token in state.Maze.tokens)
            {
                Rectangle tokenRect = new Rectangle(
                    token.x * cellW + cellW / 6,
                    token.y * cellH + cellH / 6,
                    System.Math.Max(2, 2 * cellW / 3),
                    System.Math.Max(2, 2 * cellH / 3));

                Image sprite;
                if (token.value == 5)
                {
                    sprite = GameSession.golden_token;
                }
                else if (token.value == 3)
                {
                    sprite = GameSession.silver_token;
                }
                else
                {
                    sprite = GameSession.bronze_token;
                }

                if (sprite != null)
                {
                    g.DrawImage(sprite, tokenRect);
                }
                else
                {
                    // skip drawing if asset missing so game does not crash
                }
            }
        }

        private void DrawExit(Graphics g, int cellW, int cellH)
        {
            Point exit = state.Maze.exit;
            Rectangle rect = new Rectangle(
                exit.X * cellW,
                exit.Y * cellH,
                cellW,
                cellH);
            g.FillRectangle(Brushes.Yellow, rect);
        }

        private void DrawMonster(Graphics g, int cellW, int cellH)
        {
            if (state.Monsters != null && state.Monsters.Count > 0)
            {
                for (int i = 0; i < state.Monsters.Count; i++)
                {
                    var m = state.Monsters[i];
                    if (!m.Active)
                    {
                        continue;
                    }

                    Rectangle rect = new Rectangle(
                        m.X * cellW + cellW / 6,
                        m.Y * cellH + cellH / 6,
                        System.Math.Max(2, 2 * cellW / 3),
                        System.Math.Max(2, 2 * cellH / 3));
                    g.FillEllipse(Brushes.Purple, rect);
                }
                return;
            }

            if (!state.MonsterActive)
            {
                return;
            }

            Rectangle single = new Rectangle(
                state.MonsterX * cellW + cellW / 6,
                state.MonsterY * cellH + cellH / 6,
                System.Math.Max(2, 2 * cellW / 3),
                System.Math.Max(2, 2 * cellH / 3));
            g.FillEllipse(Brushes.Purple, single);
        }

        private void DrawRunner(Graphics g, int cellW, int cellH)
        {
            if (state.PlayerX < 0 || state.PlayerY < 0)
            {
                return;
            }
            Rectangle rect = new Rectangle(
                state.PlayerX * cellW + cellW / 8,
                state.PlayerY * cellH + cellH / 8,
                System.Math.Max(2, 3 * cellW / 4),
                System.Math.Max(2, 3 * cellH / 4));
            g.FillEllipse(Brushes.Black, rect);
        }

        private void DrawLabOverlays(Graphics g, int cellW, int cellH)
        {
            if (labExplored != null && state.Maze != null)
            {
                for (int y = 0; y < state.Maze.height; y++)
                {
                    for (int x = 0; x < state.Maze.width; x++)
                    {
                        if (labExplored[x, y])
                        {
                            Rectangle rect = new Rectangle(
                                x * cellW,
                                y * cellH,
                                cellW,
                                cellH);
                            g.FillRectangle(Brushes.LightBlue, rect);
                        }
                    }
                }
            }

            if (labPath != null && state.Maze != null)
            {
                for (int y = 0; y < state.Maze.height; y++)
                {
                    for (int x = 0; x < state.Maze.width; x++)
                    {
                        if (labPath[x, y])
                        {
                            Rectangle rect = new Rectangle(
                                x * cellW,
                                y * cellH,
                                cellW,
                                cellH);
                            g.FillRectangle(Brushes.Gold, rect);
                        }
                    }
                }
            }
        }
    }
}
