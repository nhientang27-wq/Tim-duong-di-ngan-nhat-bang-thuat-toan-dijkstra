using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Do_an_DSA
{
    public class Vertex
    {
        public string Name     { get; set; }
        public bool   Visited  { get; set; }
        public string Previous { get; set; }   
        public double Distance { get; set; }   

        public Vertex(string name)
        {
            Name     = name;
            Visited  = false;
            Previous = null;
            Distance = double.MaxValue;
        }

        public void Reset()
        {
            Visited  = false;
            Previous = null;
            Distance = double.MaxValue;
        }
    }
                                                                                                                                                                                                                                                                                                                                           
    public class Location
    {
        public string Name   { get; set; }
        public string Symbol { get; set; }
        public int    X      { get; set; }
        public int    Y      { get; set; }

        public Location(string name, string symbol, int x, int y)
        {
            Name = name; Symbol = symbol; X = x; Y = y;
        }

        public Point GetPoint() => new Point(X, Y);
    }

    public class SetUpGraph
    {
        private const double INF = double.MaxValue;

        private List<Vertex> _vertices;
        private double[,]    _adj;
        private int          _maxSize;

        public List<Vertex> Vertices  => _vertices;
        public double[,]    AdjMatrix => _adj;
        public int          Count     => _vertices.Count;

        public SetUpGraph(int maxSize)
        {
            _maxSize  = maxSize;
            _vertices = new List<Vertex>();
            _adj      = new double[maxSize, maxSize];

            for (int i = 0; i < maxSize; i++)
                for (int j = 0; j < maxSize; j++)
                    _adj[i, j] = (i == j) ? 0 : INF;
        }

        public void InsertVertex(string name)
        {
            if (_vertices.Count >= _maxSize)
                throw new InvalidOperationException("Đồ thị đã đầy!");
            _vertices.Add(new Vertex(name));
        }

        public void InsertEdge(string from, string to, double weight)
        {
            int i = GetIndex(from), j = GetIndex(to);
            if (i < 0 || j < 0) return;
            _adj[i, j] = weight;
            _adj[j, i] = weight;
        }

        public int GetIndex(string name)
        {
            for (int i = 0; i < _vertices.Count; i++)
                if (_vertices[i].Name == name) return i;
            return -1;
        }

        public void Dijkstra(string startName)
        {
            foreach (var v in _vertices) v.Reset();

            int s = GetIndex(startName);
            if (s < 0) return;
            _vertices[s].Distance = 0;

            int n = _vertices.Count;
            for (int step = 0; step < n; step++)
            {
                int u = FindMinUnvisited();
                if (u < 0) break;
                _vertices[u].Visited = true;

                for (int v = 0; v < n; v++)
                {
                    if (!_vertices[v].Visited && _adj[u, v] < INF)
                    {
                        double nd = _vertices[u].Distance + _adj[u, v];
                        if (nd < _vertices[v].Distance)
                        {
                            _vertices[v].Distance = nd;
                            _vertices[v].Previous  = _vertices[u].Name;
                        }
                    }
                }
            }
        }

        private int FindMinUnvisited()
        {
            double min = INF; int idx = -1;
            for (int i = 0; i < _vertices.Count; i++)
                if (!_vertices[i].Visited && _vertices[i].Distance < min)
                { min = _vertices[i].Distance; idx = i; }
            return idx;
        }

        public List<string> FindPath(string startName, string endName)
        {
            var path = new List<string>();
            int ei = GetIndex(endName);
            if (ei < 0 || _vertices[ei].Distance >= INF) return path;

            string cur = endName;
            while (cur != null)
            {
                path.Insert(0, cur);
                cur = _vertices[GetIndex(cur)].Previous;
            }
            return path;
        }

        public double GetDistance(string name)
        {
            int i = GetIndex(name);
            return i < 0 ? INF : _vertices[i].Distance;
        }

        public bool HasPath(string name)
        {
            int i = GetIndex(name);
            return i >= 0 && _vertices[i].Distance < INF;
        }
    }

    public class MapRenderer
    {
        private List<Location> _locs;
        private double[,]      _adj;
        private List<string>   _path = new List<string>();

        private static readonly Color CRoad     = Color.FromArgb(200, 200, 210);
        private static readonly Color CNode     = Color.FromArgb(41, 98, 175);
        private static readonly Color CNodePath = Color.FromArgb(220, 80, 30);
        private static readonly Color CEdgePath = Color.FromArgb(230, 60, 40);
        private static readonly Color CLabel    = Color.FromArgb(70, 70, 80);
        private static readonly Color CBg       = Color.FromArgb(245, 247, 252);

        private const int R = 22;   

        public MapRenderer(List<Location> locs, double[,] adj)
        {
            _locs = locs; _adj = adj;
        }

        public void SetPath(List<string> path) => _path = path ?? new List<string>();

        public void Draw(Graphics g)
        {
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(CBg);

            DrawGridLines(g);
            DrawAllEdges(g);
            if (_path.Count > 1) DrawPathEdges(g);
            DrawAllNodes(g);
        }

       
        private void DrawGridLines(Graphics g)
        {
            using var p = new Pen(Color.FromArgb(20, 100, 120, 200), 1);
          
            for (int x = 0; x < 900; x += 60)
                g.DrawLine(p, x, 0, x, 700);
            for (int y = 0; y < 700; y += 60)
                g.DrawLine(p, 0, y, 900, y);
        }

        private void DrawAllEdges(Graphics g)
        {
            int n = _locs.Count;
            using var pen  = new Pen(CRoad, 2);
            using var font = new Font("Segoe UI", 7.5f);
            using var br   = new SolidBrush(CLabel);

            for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                double w = _adj[i, j];
                if (w <= 0 || w >= double.MaxValue) continue;

                var p1 = _locs[i].GetPoint();
                var p2 = _locs[j].GetPoint();
                g.DrawLine(pen, p1, p2);

                int mx = (p1.X + p2.X) / 2, my = (p1.Y + p2.Y) / 2;
                string lbl = $"{w:0.#}km";
                var sz  = g.MeasureString(lbl, font);
                var bgR = new RectangleF(mx - sz.Width/2 - 2, my - sz.Height/2 - 1,
                                         sz.Width + 4, sz.Height + 2);
                using var bgBr = new SolidBrush(Color.FromArgb(210, 245, 247, 252));
                g.FillRectangle(bgBr, bgR);
                g.DrawString(lbl, font, br, mx - sz.Width/2, my - sz.Height/2);
            }
        }

        private void DrawPathEdges(Graphics g)
        {
            using var pen = new Pen(CEdgePath, 5)
            {
                StartCap = LineCap.Round, EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            for (int i = 0; i < _path.Count - 1; i++)
            {
                var a = _locs.Find(l => l.Name == _path[i]);
                var b = _locs.Find(l => l.Name == _path[i + 1]);
                if (a == null || b == null) continue;

                g.DrawLine(pen, a.X, a.Y, b.X, b.Y);
                DrawArrow(g, a.GetPoint(), b.GetPoint());
            }
        }

        private void DrawArrow(Graphics g, Point from, Point to)
        {
            double ang = Math.Atan2(to.Y - from.Y, to.X - from.X);
            int mx = (from.X + to.X) / 2, my = (from.Y + to.Y) / 2;
            double len = 13, spread = Math.PI / 6;

            var tip = new Point(mx, my);
            var l   = new Point(mx - (int)(len * Math.Cos(ang - spread)),
                                my - (int)(len * Math.Sin(ang - spread)));
            var r   = new Point(mx - (int)(len * Math.Cos(ang + spread)),
                                my - (int)(len * Math.Sin(ang + spread)));

            using var br = new SolidBrush(CEdgePath);
            g.FillPolygon(br, new[] { tip, l, r });
        }

        private void DrawAllNodes(Graphics g)
        {
            using var fSym  = new Font("Segoe UI", 8f, FontStyle.Bold);
            using var fName = new Font("Segoe UI", 7.5f);
            using var brW   = new SolidBrush(Color.White);
            using var brLbl = new SolidBrush(Color.FromArgb(50, 50, 60));

            foreach (var loc in _locs)
            {
                bool onPath  = _path.Contains(loc.Name);
                Color fill   = onPath ? CNodePath : CNode;
                var   rect   = new Rectangle(loc.X - R, loc.Y - R, R*2, R*2);

                using var shadow = new SolidBrush(Color.FromArgb(35, 0, 0, 0));
                g.FillEllipse(shadow, new Rectangle(rect.X+3, rect.Y+3, rect.Width, rect.Height));

                using var nb = new SolidBrush(fill);
                g.FillEllipse(nb, rect);

                using var bp = new Pen(Color.White, onPath ? 3 : 2);
                g.DrawEllipse(bp, rect);

                var ss = g.MeasureString(loc.Symbol, fSym);
                g.DrawString(loc.Symbol, fSym, brW,
                    loc.X - ss.Width/2, loc.Y - ss.Height/2);

                var ns = g.MeasureString(loc.Name, fName);
          
                var nr = new RectangleF(loc.X - ns.Width/2 - 2, loc.Y + R + 3, ns.Width + 4, ns.Height);
                using var nb2 = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
                g.FillRectangle(nb2, nr);
                g.DrawString(loc.Name, fName, brLbl, loc.X - ns.Width/2, loc.Y + R + 3);
            }
        }
    }


    public partial class Form1 : Form
    {
        private const double GIA_XANG        = 21_000;   
        private const double TIEU_HAO        = 8.5;      
        private const double PHI_CAU_DUONG   = 500;      

       
        private List<Location> _locations = new();
        private SetUpGraph     _graph;
        private MapRenderer    _renderer;
        private List<string>   _path     = new();

        private Panel    pnlMap, pnlSide;
        private ComboBox cboFrom, cboTo;
        private Button   btnFind, btnReset;
        private TextBox  txtKm, txtPath, txtXang, txtCauDuong, txtTong;
        private Label    lblStatus;

        public Form1() { InitializeComponent(); BuildUI(); }


        private void BuildUI()
        {
            this.Text            = "Tối Ưu Hóa Chi Phí Vận Tải — Thuật Toán Dijkstra";
            this.Size            = new Size(1200, 730);
            this.MinimumSize     = new Size(1050, 660);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = Color.FromArgb(230, 235, 248);
            this.Font            = new Font("Segoe UI", 9.5f);

       
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(41, 98, 175)
            };
            var lblTitle = new Label
            {
                Text      = "  🗺  TÌM ĐƯỜNG ĐI NGẮN NHẤT VÀ TỐI ƯU CHI PHÍ VẬN TẢI",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblTitle);
            this.Controls.Add(header);

            
            pnlMap = new Panel
            {
                Location    = new Point(10, 58),
                Size        = new Size(740, 620),
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlMap.Paint += (s, e) => _renderer?.Draw(e.Graphics);
            this.Controls.Add(pnlMap);

            
            pnlSide = new Panel
            {
                Location    = new Point(758, 58),
                Size        = new Size(418, 620),
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlSide);

            BuildSidePanel();
        }

        private void BuildSidePanel()
        {
            int y = 18;

            
            SideSection(pnlSide, "📍  CHỌN TUYẾN ĐƯỜNG", ref y);

            SideLabel(pnlSide, "Điểm xuất phát:", 16, y);
            cboFrom = SideCombo(pnlSide, 16, y + 22, 382); y += 58;

            SideLabel(pnlSide, "Điểm đến:", 16, y);
            cboTo   = SideCombo(pnlSide, 16, y + 22, 382); y += 58;

            btnFind = new Button
            {
                Text      = "🔍   TÌM ĐƯỜNG NGẮN NHẤT",
                Location  = new Point(16, y),
                Size      = new Size(382, 40),
                BackColor = Color.FromArgb(41, 98, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnFind.FlatAppearance.BorderSize = 0;
            btnFind.Click += BtnFind_Click;
            pnlSide.Controls.Add(btnFind);
            y += 50;

            Separator(pnlSide, y); y += 16;

            
            SideSection(pnlSide, "📊  KẾT QUẢ TÍNH TOÁN", ref y);

            SideLabel(pnlSide, "Tổng quãng đường:", 16, y);
            txtKm = SideReadBox(pnlSide, 16, y + 22, 382, 28); y += 56;

            SideLabel(pnlSide, "Lộ trình chi tiết:", 16, y);
            txtPath = new TextBox
            {
                Location   = new Point(16, y + 22),
                Size       = new Size(382, 65),
                Multiline  = true,
                ReadOnly   = true,
                BackColor  = Color.FromArgb(246, 248, 255),
                ScrollBars = ScrollBars.Vertical,
                Font       = new Font("Segoe UI", 9f)
            };
            pnlSide.Controls.Add(txtPath);
            y += 94;

            Separator(pnlSide, y); y += 16;

            
            SideSection(pnlSide, "💰  CHI PHÍ VẬN TẢI ƯỚC TÍNH", ref y);

            SideLabel(pnlSide, $"Chi phí xăng ({TIEU_HAO}L/100km × {GIA_XANG:N0} VNĐ/L):", 16, y);
            txtXang = SideReadBox(pnlSide, 16, y + 22, 382, 28); y += 56;

            SideLabel(pnlSide, $"Phí cầu đường ({PHI_CAU_DUONG:N0} VNĐ/km):", 16, y);
            txtCauDuong = SideReadBox(pnlSide, 16, y + 22, 382, 28); y += 56;

            SideLabel(pnlSide, "TỔNG CHI PHÍ:", 16, y, bold: true, color: Color.FromArgb(41,98,175));
            txtTong = new TextBox
            {
                Location  = new Point(16, y + 22),
                Size      = new Size(382, 32),
                ReadOnly  = true,
                BackColor = Color.FromArgb(255, 248, 220),
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 60, 0)
            };
            pnlSide.Controls.Add(txtTong);
            y += 62;

            Separator(pnlSide, y); y += 12;

            
            btnReset = new Button
            {
                Text      = "🔄  Làm mới",
                Location  = new Point(16, y),
                Size      = new Size(120, 32),
                BackColor = Color.FromArgb(210, 215, 225),
                ForeColor = Color.FromArgb(50, 50, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            pnlSide.Controls.Add(btnReset);

            lblStatus = new Label
            {
                Location  = new Point(148, y + 6),
                Size      = new Size(250, 22),
                ForeColor = Color.Gray,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Text      = "Sẵn sàng."
            };
            pnlSide.Controls.Add(lblStatus);
        }

        
        private void SideSection(Panel p, string text, ref int y)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 98, 175),
                Location  = new Point(16, y),
                Size      = new Size(390, 22)
            };
            p.Controls.Add(lbl);
            y += 26;
        }

        private void SideLabel(Panel p, string text, int x, int y,
                               bool bold = false, Color? color = null)
        {
            var lbl = new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                AutoSize  = true,
                ForeColor = color ?? Color.FromArgb(55, 55, 65),
                Font      = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular)
            };
            p.Controls.Add(lbl);
        }

        private ComboBox SideCombo(Panel p, int x, int y, int w)
        {
            var c = new ComboBox
            {
                Location      = new Point(x, y),
                Size          = new Size(w, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            p.Controls.Add(c);
            return c;
        }

        private TextBox SideReadBox(Panel p, int x, int y, int w, int h)
        {
            var t = new TextBox
            {
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                ReadOnly  = true,
                BackColor = Color.FromArgb(246, 248, 255)
            };
            p.Controls.Add(t);
            return t;
        }

        private void Separator(Panel p, int y) =>
            p.Controls.Add(new Panel
            {
                Location  = new Point(16, y),
                Size      = new Size(390, 1),
                BackColor = Color.FromArgb(195, 205, 225)
            });

        
        private void Form1_Load(object sender, EventArgs e)
        {
            InitLocations();
            InitGraph();
            InitComboBoxes();
            _renderer = new MapRenderer(_locations, _graph.AdjMatrix);
            pnlMap.Invalidate();
        }

        private void InitLocations()
        {
            
            _locations.Add(new Location("Quận 1",     "Q1",  370, 330));
            _locations.Add(new Location("Quận 3",     "Q3",  290, 265));
            _locations.Add(new Location("Quận 4",     "Q4",  370, 415));
            _locations.Add(new Location("Quận 5",     "Q5",  230, 345));
            _locations.Add(new Location("Quận 6",     "Q6",  145, 390));
            _locations.Add(new Location("Quận 7",     "Q7",  310, 510));
            _locations.Add(new Location("Quận 8",     "Q8",  200, 460));
            _locations.Add(new Location("Quận 10",    "Q10", 205, 265));
            _locations.Add(new Location("Quận 12",    "Q12", 210, 115));
            _locations.Add(new Location("Bình Thạnh", "BTh", 465, 220));
            _locations.Add(new Location("Tân Bình",   "TBi", 155, 195));
            _locations.Add(new Location("Gò Vấp",     "GV",  315, 150));
            _locations.Add(new Location("Thủ Đức",    "TĐ",  590, 175));
            _locations.Add(new Location("Nhà Bè",     "NB",  460, 530));
        }

        private void InitGraph()
        {
            _graph = new SetUpGraph(_locations.Count);
            foreach (var loc in _locations) _graph.InsertVertex(loc.Name);

            
            var edges = new (string a, string b, double km)[]
            {
                ("Quận 1",  "Quận 3",      2.1),
                ("Quận 1",  "Quận 4",      2.5),
                ("Quận 1",  "Quận 5",      3.3),
                ("Quận 1",  "Bình Thạnh",  3.9),
                ("Quận 3",  "Quận 5",      2.0),
                ("Quận 3",  "Quận 10",     2.7),
                ("Quận 3",  "Tân Bình",    4.4),
                ("Quận 3",  "Bình Thạnh",  4.1),
                ("Quận 3",  "Gò Vấp",      5.5),
                ("Quận 4",  "Quận 7",      4.2),
                ("Quận 4",  "Quận 8",      3.5),
                ("Quận 5",  "Quận 6",      2.6),
                ("Quận 5",  "Quận 10",     2.2),
                ("Quận 6",  "Quận 8",      2.8),
                ("Quận 7",  "Nhà Bè",      5.8),
                ("Quận 7",  "Quận 8",      4.5),
                ("Quận 8",  "Quận 6",      2.8),
                ("Quận 10", "Tân Bình",    3.4),
                ("Quận 10", "Quận 12",     5.1),
                ("Quận 12", "Tân Bình",    4.3),
                ("Quận 12", "Gò Vấp",      3.7),
                ("Tân Bình","Gò Vấp",      5.0),
                ("Gò Vấp",  "Bình Thạnh",  4.2),
                ("Gò Vấp",  "Thủ Đức",     8.8),
                ("Bình Thạnh","Thủ Đức",   7.1),
                ("Bình Thạnh","Quận 1",    3.9),
                ("Nhà Bè",  "Quận 4",      6.5),
            };

            foreach (var (a, b, km) in edges)
                _graph.InsertEdge(a, b, km);
        }

        private void InitComboBoxes()
        {
            foreach (var loc in _locations)
            {
                cboFrom.Items.Add(loc.Name);
                cboTo.Items.Add(loc.Name);
            }
            cboFrom.SelectedIndex = 0;
            cboTo.SelectedIndex   = 1;
        }

       
        private void BtnFind_Click(object sender, EventArgs e)
        {
            string from = cboFrom.SelectedItem?.ToString();
            string to   = cboTo.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                Status("⚠  Vui lòng chọn đầy đủ điểm đi và điểm đến.", Color.OrangeRed);
                return;
            }
            if (from == to)
            {
                Status("⚠  Điểm đi và điểm đến không được trùng nhau!", Color.OrangeRed);
                ClearResults(); ReDrawMap(); return;
            }

            _graph.Dijkstra(from);

            if (!_graph.HasPath(to))
            {
                Status($"❌  Không có đường đi từ [{from}] đến [{to}].", Color.Red);
                ClearResults(); ReDrawMap(); return;
            }

            double km = _graph.GetDistance(to);
            _path = _graph.FindPath(from, to);

           
            txtKm.Text = $"{km:0.##} km";

            var sb = new StringBuilder();
            for (int i = 0; i < _path.Count; i++)
            {
                if (i > 0) sb.Append(" → ");
                sb.Append(_path[i]);
            }
            txtPath.Text = sb.ToString();

          
            double xang   = km * TIEU_HAO / 100 * GIA_XANG;
            double cau    = km * PHI_CAU_DUONG;
            double tong   = xang + cau;

            txtXang.Text     = $"{xang:N0} VNĐ";
            txtCauDuong.Text = $"{cau:N0} VNĐ";
            txtTong.Text     = $"{tong:N0} VNĐ";

            Status($"✅  Đường ngắn nhất: {km:0.##} km  |  {_path.Count - 1} chặng", Color.Green);

            ReDrawMap();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            cboFrom.SelectedIndex = 0;
            cboTo.SelectedIndex   = 1;
            ClearResults();
            _path.Clear();
            ReDrawMap();
            Status("Sẵn sàng.", Color.Gray);
        }

        
        private void ClearResults()
        {
            txtKm.Text = txtPath.Text = txtXang.Text =
            txtCauDuong.Text = txtTong.Text = string.Empty;
        }

        private void Status(string msg, Color c)
        {
            lblStatus.Text      = msg;
            lblStatus.ForeColor = c;
        }

        private void ReDrawMap()
        {
            _renderer?.SetPath(_path);
            pnlMap.Invalidate();
        }
    }
}
