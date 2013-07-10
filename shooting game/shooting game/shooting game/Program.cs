using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;



public class Shooting : Game
{
    public static void Main(string[] arg)
    {
        using (Game g = new Shooting())
        {
            g.Run();
        }

    }

    private GraphicsDeviceManager Gm;
    private SpriteBatch sprite;
    private ContentManager Cm;
    private Jiki jiki;
    private Jshoot jshoot;
    private Backmap backmap;
    private Enemy enemy;
    private Message msg;
    public static int StageFlg;
    //    private Texture2D Tbackmap;
    //private Shooting Parent;
    //private Texture2D Tjiki;
    Shooting()
    {
        Gm = new GraphicsDeviceManager(this);
        Gm.PreferredBackBufferWidth = 300;
        Gm.PreferredBackBufferHeight = 400;
        StageFlg=0;
        
    }

    protected override void LoadContent()
    {
        sprite=new SpriteBatch(GraphicsDevice);
        Cm = new ContentManager(Services);
        jiki=new Jiki(this,Cm,sprite);
        jshoot=new Jshoot(Cm,sprite);
        backmap=new Backmap(Cm,sprite);
        enemy= new Enemy(this,Cm,sprite);
        msg=new Message(sprite,Cm);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Inkey();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.Clear(Color.Black);
        sprite.Begin();
        backmap.paintMap();
        jiki.paintJiki();
        jshoot.xy_calc();
        jshoot.paintShoot();
        switch(StageFlg){
            case 1:enemy.SetEnemy();
                jiki.JikiVsEnemy();
                jshoot.JShootVsEnemy();
                enemy.EnemyMove(jiki.retX(),jiki.retY());
                enemy.paintEnemy();
                jshoot.blastMap();
                jiki.blastMap();
                break;
            case 2:
            case 3:
                enemy.EnemyMove(jiki.retX(),jiki.retY());
                enemy.paintEnemy();
                break;

        }
        msg.paintGameMsg(StageFlg);
        msg.paintScore();
        jiki.paintLife();
        sprite.End();
        base.Draw(gameTime);
    }
    public void Inkey()
    {
    KeyboardState state=Keyboard.GetState();
    if(state[Keys.Up   ] == KeyState.Down)jiki.moveU();
    if(state[Keys.Down ] == KeyState.Down)jiki.moveD();
    if(state[Keys.Left ] == KeyState.Down)jiki.moveL();  
    if(state[Keys.Right] == KeyState.Down)jiki.moveR();
    if(state[Keys.X    ] == KeyState.Down)StageFlg = 1;
    if(state[Keys.Space] == KeyState.Down)jshoot.setShoot(jiki.retX(), jiki.retY());
    }

}
class Jiki:GameData
{
    const int JikiW=80;
    const int JikiH=60;
    private int JikiX,JikiY;
    private int speedX,speedY;
    private SpriteBatch sprite;
    private Texture2D Tjiki;
    private Shooting Parent;
    private Blast blast;
    
    public Jiki(Shooting _parent,ContentManager _c,SpriteBatch _sprite)
    {
        JikiX=160;JikiY=300;
        speedX=5;speedY=5;
        sprite= _sprite;
        Parent=_parent;
        Tjiki=_c.Load<Texture2D>("content/jiki");
        blast=new Blast(_c,_sprite);
    }

    public void moveU(){if(JikiY>10)     JikiY -= speedY;}
    public void moveD(){if(JikiY<400-50) JikiY += speedY;}
    public void moveR(){if(JikiX<300-50) JikiX += speedX;}
    public void moveL(){if(JikiX>0)      JikiX -= speedX;}
    
    public void paintJiki()
    {
    Vector2 pos;
        pos.X=JikiX;    pos.Y=JikiY;
        sprite.Draw(Tjiki,pos,Color.White);
    }
    private int JikiLife = 3;
    public void paintLife()
    {
        Rectangle rect;
        rect.X = 250;
        rect.Width = 32; rect.Height = 32;
        for (int i = 1; i < JikiLife; i++)
        {
            rect.Y = 10 + i * 35;
            sprite.Draw(Tjiki, rect, Color.White);
        }
    }
    public int retX(){return JikiX;}
    public int retY(){return JikiY;}

    public void JikiVsEnemy()
    {
        int jx, jy, ex, ey, r, p;
        double wx, wy;
        jx = JikiX + 25;
        jy = JikiY + 25;
        for (int i = 0; i < MAPENEMYSU; i++)
        {
            if (EMapNo[i] != -1)
            {
                p = EMapNo[i];
                ex = Ex[p] + 16; ey = Ey[p]+16;
                wx = Math.Abs(jx - ex); wy = Math.Abs(jy - ey);
                r = (int)Math.Sqrt(wx * wx * +wy * wy);
                if (r < (20 + 12))
                {
                    blast.setBlast(Ex[p] - 32, Ey[p] - 32);
                    EMapNo[i]=-1;
                    JikiLife-=1;
                    if(JikiLife<0)Shooting.StageFlg=3;
                    return;
                }
            }
        }
    }
    public void blastMap()
    {
        blast.paintBlast();
    }
}

class Jshoot : GameData
{
    const int SHOOTSU = 5;
    private int[] x, y;
    private int speedY;
    private bool[] sw;
    private Texture2D Tmissile;
    private SpriteBatch sprite;
    private Blast blast;

    public Jshoot(ContentManager _c, SpriteBatch _sprite)
    {
        sprite = _sprite;
        x = new int[SHOOTSU];
        y = new int[SHOOTSU];
        sw = new bool[SHOOTSU];
        for (int i = 0; i < SHOOTSU; i++) sw[i] = false;
        speedY = 5;
        Tmissile = _c.Load<Texture2D>("content/jshoot");
        blast = new Blast(_c, _sprite);
    }

    public void xy_calc()
    {
        for (int i = 0; i < SHOOTSU; i++)
            if (sw[i] == true)
            {
                y[i] -= speedY;
                if (y[i] < -20) sw[i] = false;
            }
    }

    int waitcnt = 0;
    public void setShoot(int jx, int jy)
    {

        if (waitcnt < 5)
        {
            waitcnt++; return;
        }
        waitcnt = 0;

        for (int i = 0; i < SHOOTSU; i++)
            if (sw[i] == false)
            {
                sw[i] = true;
                x[i] = jx + 18;
                y[i] = jy - 15;
                return;
            }
        return;

    }

    public void paintShoot()//ミサイルの描画
    {
        Vector2 pos;
        for (int i = 0; i < SHOOTSU; i++)
            if (sw[i] == true)
            {
                pos.X = x[i]; pos.Y = y[i];
                sprite.Draw(Tmissile, pos, Color.White);
            }
    }


    public void JShootVsEnemy()
    {
        int sx, sy, ex, ey, p;

        for (int i = 0; i < SHOOTSU; i++)
        {
            if (sw[i] == true)
            {
                for (int j = 0; j < MAPENEMYSU; j++)
                {
                    p = EMapNo[j];
                    if (p != -1)
                    {
                        sx = x[i] + 7; sy = y[i] + 16;
                        ex = Ex[p]; ey = Ey[p];
                        if (ey < sy && sy < ey + 32)
                            if (ey < sy && sy < ey + 32)
                            {
                                EMapNo[i] = -1;
                                sw[i] = false;
                                blast.setBlast(sx - 40, sy - 40);
                                HitPoint++;
                            }

                    }
                }
            }
        }
    }
    public void blastMap()
    {
        blast.paintBlast();
    }
}

class Blast
{
    const int BLASTSU = 5;
    private int[] x, y, mapNo;
    private Texture2D[] TBlast;
    private SpriteBatch sprite;

    public Blast(ContentManager _c, SpriteBatch _sprite)
    {
        TBlast = new Texture2D[10];
        for (int i = 0; i < 10; i++)
        {
            TBlast[i] = _c.Load<Texture2D>("content/B" + i);

            sprite = _sprite;

            x = new int[BLASTSU];
            y = new int[BLASTSU];
            mapNo = new int[BLASTSU];
            for (int j = 0; j < BLASTSU; j++)
            {
                mapNo[j] = -1;
            }

        }
    }

    public void setBlast(int _x, int _y)
    {
        for (int i = 0; i < BLASTSU; i++)
        
            if (mapNo[i] == -1)
            {
                x[i] = _x; y[i] = _y;
                mapNo[i]=0;
                break;
            }
    }
     public void animNoUpdate()
     {
         for(int i=0;i<BLASTSU;i++)
             if(mapNo[i]!=-1)
             {
                 mapNo[i]++;
                 if(mapNo[i]>9)mapNo[i]=-1;
             }
         
     }

     public void paintBlast()
     {
         int no;
         Vector2 pos;

         for(int i=0;i<BLASTSU;i++)
             if(mapNo[i]!=-1)
         {
             no = mapNo[i];
             pos.X = x[i]; pos.Y = y[i];
             sprite.Draw(TBlast[no], pos, Color.White);
         }
         animNoUpdate();
     }
    
    }

class Enemy : GameData
{
    private Texture2D Tteki;
    private SpriteBatch sprite;
    private Shooting Parent;
    public Enemy(Shooting _parent, ContentManager _c, SpriteBatch _sprite)
    { 
    Parent =_parent;
    sprite = _sprite;
    Tteki = _c.Load<Texture2D>("content/enemy");
    //for (int i = 0; i < MAPENEMYSU; i++)
    //    EMapNo[i] = -1;
    //    setCount = 0;
    }
    private int setCount = 0;
    public void SetEnemy() { 
    setCount++;
    if(setCount<10)return;
        setCount=0;

        if(EnemyDataPoint>=ENEMYSU){
        //Parent.setStageFlg=2;
        Shooting.StageFlg=2;
        return;
    }
        for(int i=0;i<MAPENEMYSU;i++){
        if(EMapNo[i]==-1){
        EMapNo[i]=EnemyDataPoint;
        EnemyDataPoint++;
        return;
    }
  }
        return;
}

public void paintEnemy()
{
    int i,p;
    Vector2 pos;

    for(i=0;i<MAPENEMYSU;i++)
        if(EMapNo[i]!=-1){
            p=EMapNo[i];
            pos.X=Ex[p];
            pos.Y=Ex[p];
            sprite.Draw(Tteki,pos,Color.White);
        }
}

    public void EnemyMove(int jx,int jy){
        int p,mtype;
        for(int i=0;i<MAPENEMYSU;i++){
            if(EMapNo[i]!=-1){
                p=EMapNo[i];

                mtype=Ey[p];
                switch(mtype){
                    case 0:
                        Ex[p]+=Espeedx[p];
                        Ey[p]+=Espeedy[p];
                        break;
                    case 1:
                        option[p]++;
                        if(option[p]==100){
                            //Espeedx[p]=0;
                            //Espeedy[p]=5;
                            setSpeed(p,jx,jy);
                        }
                        Ex[p]+=Espeedx[p];
                        Ey[p]+=Espeedx[p];

                        break;
                }
                if(Ex[p]<-20||Ex[p]>300||Ey[p]<-20||Ey[p]>400)
                    EMapNo[i]=-1;
            }
        }
    }

    public void setSpeed(int p,int jx,int jy)
    {
        int wx,wy,saX,saY;
        double wRadian;
        wx=Ex[p]-(jx+30);
        wy=Ey[p]-(jy+30);
        saX=Math.Abs(wx);
        saY=Math.Abs(wy);

        wRadian=Math.Atan2((double)saY,(double)saX);
        Espeedx[p]=(int)(Math.Cos(wRadian)*5.0);
        Espeedy[p]=(int)(Math.Sin(wRadian)*5.0);

        if(wx<0&& wy>0){
            Espeedy[p] *=-1;
        }else
            if(wx>0&&wy>0){
                Espeedx[p] *=-1;
                Espeedy[p] *=-1;
                
            }else
                if(wx>0&&wy<0){
                Espeedx[p] *=-1;
                }
    }

}//Enemy

class Message:GameData
{
    private SpriteBatch sprite;
    private SpriteFont wfont1,wfont2;

    public Message(SpriteBatch _sprite,ContentManager _Cm)
    {
        sprite= _sprite;
        wfont1= _Cm.Load<SpriteFont>("Content/MS20");
        wfont2= _Cm.Load<SpriteFont>("Content/MS30");
    }
    public void paintScore()
{
    Vector2 pos;
    pos.X=10;
    pos.Y=10;
    sprite.DrawString(wfont1,"Score:"+(HitPoint*10),pos,Color.Yellow);
}
    public void paintGameMsg(int flg)
    {
        Vector2 pos;
        pos.X=50;
        pos.Y=100;
        switch(flg){
            case 0: sprite.DrawString(wfont1,"Push X Key",pos,Color.Yellow);
                break;
            case 2: sprite.DrawString(wfont2,"Game Clear!",pos,Color.Yellow);
                break;
            case 3: sprite.DrawString(wfont2,"Game Over!",pos,Color.Yellow);
                break;
        }
    }
}

class Backmap
{
    private Texture2D Tbackmap;
    private SpriteBatch sprite;

    public Backmap(ContentManager _c,SpriteBatch _sprite)
    {
        sprite=_sprite;
        Tbackmap=_c.Load<Texture2D>("content/BackMap");
    }

    public void paintMap()
    {
        sprite.Draw(Tbackmap,Vector2.Zero,Color.White);
    }
}

class GameData
{
    public const int MAPENEMYSU=5;
    public const int ENEMYSU=60;

    public static int HitPoint =0;
    public static int GameFlg=0;

    //敵データ格納領域
    public static int[] Emovetype=new int[ENEMYSU];
    public static int[] Ex       =new int[ENEMYSU];
    public static int[] Ey       =new int[ENEMYSU];
    public static int[] Espeedx  =new int[ENEMYSU];
    public static int[] Espeedy  =new int[ENEMYSU];
    public static int[] option   =new int[ENEMYSU];

    //画面描画をする為のデータ保存領域
    public static int[] EMapNo=new int[MAPENEMYSU];

    public static int EnemyDataPoint;

    public GameData(){
    int w=0;
        EnemyDataPoint=0;

        //EmapNoの初期設定
        for(int i=0;i<MAPENEMYSU;i++)
            EMapNo[i]=-1;
//パターン1 0:上→下 1:自機に向かう隕石
    Emovetype[w]=0; Ex[w]=10;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=60;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=4; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=100; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=150; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=5; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=240; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;

//パターン2
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=100; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=100; Ey[w]=30;  Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=190; Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=260; Ey[w]=100; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;

//パターン3
    Emovetype[w]=0; Ex[w]=20;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=70;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=130; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=70;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=130; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;

//パターン4
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=120; Ey[w]=100; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=220; Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=260; Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;

//パターン5
    Emovetype[w]=0; Ex[w]=20;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=80;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=150; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=200; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=250; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    
//パターン6
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=100; Ey[w]=100; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=190; Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=250; Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    
//パターン1 0:上→下 1:自機に向かう隕石
    Emovetype[w]=0; Ex[w]=10;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=60;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=4; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=100; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=1; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=150; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=5; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=240; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;

//パターン2
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=0;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=100; Ey[w]=50; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=190; Ey[w]=50; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=260; Ey[w]=0;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    
//パターン3
    Emovetype[w]=1; Ex[w]=20;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=70;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=130; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=70;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=130; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;

//パターン4
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=120; Ey[w]=100; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=220; Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=260; Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;

//パターン5
    Emovetype[w]=0; Ex[w]=20;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=80;  Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=150; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=200; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    Emovetype[w]=0; Ex[w]=250; Ey[w]=0; Espeedx[w]=0; Espeedy[w]=2; option[w]=0; w++;
    
//パターン6
    Emovetype[w]=1; Ex[w]=10;  Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=50;  Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=100; Ey[w]=100; Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=190; Ey[w]=50;  Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    Emovetype[w]=1; Ex[w]=250; Ey[w]=0;   Espeedx[w]=0; Espeedy[w]=0; option[w]=0; w++;
    }
}










