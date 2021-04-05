using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace calc
{
    public partial class Form1 : Form
    {
        public static Potatoe BigHack;
        Overlays o;
        Thread t;
        Thread g;

        public static bool b_MouseDown = true;
        public static bool b_KeyboardDown = false;
        public static Keys k_AimBotToggleKey = Keys.Q;

        public Form1()
        {
            InitializeComponent();
            o = new Overlays();
            t = new Thread(Activate);
            g = new Thread(Activate);

            label1.Text = "Offline.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                BigHack = new Potatoe();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.InnerException.Message, ex.Message);
                label1.Text = "Error...";
                return;
            }

            KeyboardHook.Start();

            bool aim = false;
            bool recoil = false;

            g = new Thread(() => 
            {
                while (true)
                {
                    BigHack.Get();
                    if (checkBox2.Checked && b_KeyboardDown)
                        aim = BigHack.AimBot();
                    if (checkBox3.Checked)
                        recoil = BigHack.AntiRecoil();
                    if ((aim || recoil) && b_MouseDown)
                        BigHack.Set();
                } 
            });
            g.Start();

            label1.Text = "Online.";
            panel1.Enabled = true;
            button1.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                o = new Overlays();
                t = new Thread(o.Run);
                t.Start();
            }
            else
            {
                o.Dispose();
                t.Abort();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 1)
            {
                k_AimBotToggleKey = (Keys)textBox1.Text.ToUpper()[0];
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "Updating...";
            Reset();

            using (WebClient client = new WebClient())
            {
                string s = client.DownloadString("https://raw.githubusercontent.com/frk1/hazedumper/master/csgo.json");
                if (!File.Exists(@"csgo.json"))
                {
                    File.WriteAllText(@"csgo.json", s);
                    MessageBox.Show("'csgo.json' has been created in the same directory.", "Updated Successfully!");
                    label1.Text = "File Created!";
                    return;
                }
                var hold = JsonConvert.DeserializeObject<Hazedump>(File.ReadAllText(@"csgo.json"));
                var hnew = JsonConvert.DeserializeObject<Hazedump>(s);

                if (hold.timestamp == hnew.timestamp)
                {
                    MessageBox.Show("i think", "Already up to date!");
                    label1.Text = "Offline.";
                }
                else
                {
                    File.WriteAllText(@"csgo.json", s);
                    label1.Text = "Updated!";
                    MessageBox.Show("OK", "Updated Successfully!");
                }
            }
        }

        public void Reset()
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;

            panel1.Enabled = false;
            button1.Enabled = true;
            deathmatch_clock.Enabled = false;

            if (checkBox1.Checked)
                o.Dispose();
            if (t.IsAlive)
                t.Abort();
            if (g.IsAlive)
                g.Abort();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Calculator v2.1\nMade by a handsome lad called Foopy.\nCrashing every 5 min is an intended feature.", "About");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            label1.Text = "Exiting...";
            KeyboardHook.Stop();
            if (checkBox1.Checked)
                o.Dispose();
            if (t.IsAlive)
                t.Abort();
            if (g.IsAlive)
                g.Abort();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Offline.";
            Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BigHack.ResetPlayers();
        }

        private void deathmatch_clock_Tick(object sender, EventArgs e)
        {
            BigHack.ResetPlayers();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            deathmatch_clock.Enabled = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                b_MouseDown = false;
                MouseHook.Start();
            }
            else
            {
                b_MouseDown = true;
                MouseHook.Stop();
            }
        }
    }
}
