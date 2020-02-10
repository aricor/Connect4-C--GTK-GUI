using Cairo;
using Gdk;
using Gtk;
using System;
using static System.Console;
using static System.Math;

using Color = Gdk.Color;
using Rectangle = Cairo.Rectangle;
using Window = Gtk.Window;

delegate void Notify();

// A model class for a game of Tic Tac Toe.
class Board {
  int[,] square = new int[7, 6];  // 0 = empty, 1 = player 1, 2 = player 2
  
  public int player = 1;   // whose turn it is
  public int winner = 0;   // who has won
  public int win_x, win_y, win_dx, win_dy;  // vector indicating winning squares
  
  public event Notify changed;  // fires whenever the board changes
  
  public int this[int x, int y] {
    get => square[x, y];
  }
  
  // Return true if the current player has three in a row,
  // starting at (x, y) and moving in direction (dx, dy).
  bool all(int x, int y, int dx, int dy, int k ) {
        int count = 0;
        int x1 = x;
        int y1 = y;
        for (int i = 0; i < k; ++i) {
            if (square[x + i * dx, y + i * dy] == player) {
                count++;
                if (count == 4) {
                    win_x = x1;
                    win_y = y1;
                    win_dx = dx;
                    win_dy = dy;
                    return true;
				}
            }
			else {
                x1 = x + (i + 1) * dx;
                y1 = y + (i + 1) * dy;
                count = 0;
            }
        }
        return false;
  }

    // Return true if the current player has won.
    bool checkWin()
    {
        // check rows and columns
        for (int i = 0; i < 6; ++i)
            if (all(0, i, 1, 0, 7))
                return true;
        for (int i = 0; i < 7; ++i)
			if (all(i, 0, 0, 1, 6))
            return true;
        // check diagonals
        for (int i = 1; i < 4; ++i)
        {
            if (all(i, 0, 1, 1, 7 - i) || all(6 - i, 0, -1, 1, 7 - i))
                return true;
            if (all(0, i - 1, 1, 1, 7 - i) || all(6, i - 1, -1, 1, 7 - i))
                return true;
        }
        return false;
    }
  
  // Make the current player play at (x, y).  Return true if the move was legal.
  public bool move(int col) {

	for (int row = 5; row >=0; row--) {
		if (square[col, row] == 0) {
			square[col,row] =  player;
			if (checkWin()) 
				winner = player;
			else player = 3 - player;
			changed();
			return true; 
			}
		}
	return false;

  }
}


static class Util {
  public static Color parse(string name) {
    Color c = new Color(0, 0, 0);
    if (!Color.Parse(name, ref c))
      throw new Exception("bad color");
    return c;
  }
  
  // extension methods on Context
  
  public static void setColor(this Context c, Color color) {
    CairoHelper.SetSourceColor(c, color);
  }
  
  public static void drawLine(this Context c, double x1, double y1, double x2, double y2) {
    c.MoveTo(x1, y1);
    c.LineTo(x2, y2);
    c.Stroke();
  }
}

// A graphical interface for Tic Tac Toe.

class View : DrawingArea {
  const int SquareSize = 100;
  const int Margin = SquareSize / 2;
  
  Board board;
  
  void init() {
    board = new Board();
    QueueDraw();   // draw the initial position
    board.changed += QueueDraw;
  }
  
  public View() {
    init();
    AddEvents((int) EventMask.ButtonPressMask);  // ask to receive button press events
    ModifyBg(StateType.Normal, Util.parse("white"));
  }

  void drawX(Context c, Rectangle r) {
	c.setColor(Util.parse("Red"));

	 c.Arc(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width / 2, 0.0, 2 * PI);
	 c.Fill();
  }
  
  void drawO(Context c, Rectangle r) {
	c.setColor(Util.parse("Yellow"));

	 c.Arc(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width / 2, 0.0, 2 * PI);
	 c.Fill();
  }
  
  void drawCircle(Context c, Rectangle r) { 
	 c.Arc(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width / 2, 0.0, 2 * PI);
	 c.Fill();
	  }
	  	  
  void drawC(Context c) {
	for (int row = 0; row <= 600; row+=100)
		for (int col = 0; col <= 700; col+=100) {
			Rectangle rect = new Rectangle(row+10, col+10, 80, 80);
			drawCircle(c, rect); 
			}		  
	  }
	  
  protected override bool OnButtonPressEvent(EventButton ev) {
    if (board.winner > 0)
      init();   // start a new game
    else {
      int x = ((int) (ev.X - Margin)) / SquareSize;
      //int y = ((int) (ev.Y - Margin)) / SquareSize;
      board.move(x);
    }
    return true;
  }

  protected override bool OnExposeEvent(EventExpose ev) {
    Context c = CairoHelper.Create(GdkWindow);
    //draw the grid

	c.setColor(Util.parse("Blue"));

	for (int row = 50; row < 600; row+=100)
		for (int col = 50; col < 700; col+=100) {
			c.Rectangle(col, row, 100,100); 
			c.Fill(); 
			}
    //Set a transformation matrix so that squares are 1 unit tall/wide, with a
    // margin around the drawing area.
    c.Translate(Margin, Margin);
    c.Scale(SquareSize, SquareSize);
     
    
    if (board.winner > 0) {
      c.setColor(Util.parse("light green"));
      for (int i = 0 ; i < 4 ; ++i) {
        c.Rectangle(board.win_x + i * board.win_dx,
                    board.win_y + i * board.win_dy,
                    1, 1);
        c.Fill();
      }
    //  c.setColor(Util.parse("black"));
    }
    
    //draw white circles 
        for (int x = 0; x <= 6; ++x)
        {
            for (int y = 0; y <= 5; ++y) {
				c.setColor(Util.parse("Blue"));
				Rectangle r = new Rectangle(x + 0.1, y + 0.1, 0.8, 0.8);
				c.Fill(); 
				c.setColor(Util.parse("White"));
				c.Arc(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width / 2, 0.0, 2 * PI);
				c.Fill(); 
				}   
        }

        for (int x = 0; x < 7; ++x)
            for (int y = 0; y < 6; ++y)
            {
                Rectangle rect = new Rectangle(x + 0.1, y + 0.1, 0.8, 0.8);

                switch (board[x, y])
                {
                    case 1: drawX(c, rect); break;
                    case 2: drawO(c, rect); break;
                }
            }
    c.GetTarget().Dispose();
    c.Dispose();
    return true;
    }
}
			
	

	//c.setColor(Util.parse("Black")); 

class Frame : Window {
  Frame() : base("connect 4") {
    Resize(800, 700);
    Add(new View());
  }
  
  protected override bool OnDeleteEvent(Event ev) {
    Application.Quit();
    return true;
  }
  
  static void Main() {
    Application.Init();
    new Frame().ShowAll();
    Application.Run();
  }
}

