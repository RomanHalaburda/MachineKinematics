﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Printing;

namespace MachineKinematics
{
    public partial class MainForm : Form
    {
        public MainForm() { InitializeComponent(); }

        private PrintDocument printDocument = new PrintDocument();

        // flag for access to all TabPages
        Boolean accessFlag = false;

        // input data
        double L0 = 0; // Lob
        double L1 = 0; // Loa
        double L3 = 0; // Lbc
        double L5 = 0; // Lbs3
        double fi_zero = 0; // pseudo-constant
        double fi_clone = 0; // for iterations
        const double delta_fi = 2F;
        const UInt16 dimension = 181;
        double[] fi_array = new double[dimension];
        double m3 = 0;
        double m4 = 0;
        double Is4 = 0;
        double omega_1cp = 0;
        double delta = 0;
        double I_0_p = 0;
        double sign_omega1 = 1; // added 16.03.2016

        // added 19.03.2017
        double m1 = 0;
        double m5 = 0;
        double Is3 = 0;
        double Is1 = 0;
        double psi = 0;
        double eta1 = 0;

        // added at start
        double[] Fpc = new double[dimension]; // the forces of useful resistance
        
        // part 1.1 (first step)
        double[] Xa = new double[dimension];
        double[] Ya = new double[dimension];
        double[] L = new double[dimension];
        double[] cos_fi3 = new double[dimension];
        double[] sin_fi3 = new double[dimension];
        double[] fi3 = new double[dimension];
        double[] Xc = new double[dimension];
        double[] Yc = new double[dimension];
        double[] Xs3 = new double[dimension];
        double[] Ys3 = new double[dimension];

        // part 1.2 (second step)
        double[] Xa_dash = new double[dimension]; // Uax 
        double[] Ya_dash = new double[dimension]; // Uay 
        double[] Ua3a2 = new double[dimension];
        double[] i31 = new double[dimension];
        double[] Sd = new double[dimension]; // SD - slider movement
        double[] Xc_dash = new double[dimension];  // Ucx
        double[] Yc_dash = new double[dimension];  // Ucy 
        double[] Uc = new double[dimension];
        double[] Xs3_dash = new double[dimension]; // Us3x 
        double[] Ys3_dash = new double[dimension]; // Us3y
        double[] Us3 = new double[dimension];
        double[] Us5 = new double[dimension];
        double[] i31_dash = new double[dimension];

        // added 06.03.2017
        double[] Xc_doubledash = new double[dimension];
        double[] Yc_doubledash = new double[dimension];

        // part 1.3 (third step)
        // added 27.02.2017
        double[] Xs3_doubledash = new double[dimension];
        double[] Ys3_doubledash = new double[dimension];
        double[] is51_dash = new double[dimension];

// part 2.*
// added 16.03.2017
        // 2.1
        double[] M_c_pi = new double[dimension];
        double[] A_c_i = new double[dimension];
        double M_p_D = new double();

        double G2 = 0;     // in our case (machine)
        const double g = 9.8196; // Acceleration of free fall on the Earth's surface
        double G3 = 0;           // not const, calculate in general block
        double G4 = 0;     // in our case (machine)

        double[] Ys2_dash = new double[dimension]; // == Ya_dash
        double[] Ys4_dash = new double[dimension]; // == Yc_dash
        double[] Xd_dash = new double[dimension];  // == Xc_dash

        // 2.2
        double[] I_pa_second = new double[dimension];
        double[] differential_d_Ip_d_fi1 = new double[dimension];

        double A, B, C, D, E, F, G;  // temp variables for calculating the I_pa_second
        double Xs2_dash = 0;         // == Xa_dash
                                     // Ys2_dash == Ya_dash
        const double Is2 = 0;        // const for our case (machine)
        double i21 = 0;              // == i31
        const double m2 = 0;         // const for our case (machine)
        double Xs4_dash = 0;
                                     // Ys4_dash - now define
                                     // m4 = 0 - now define, 0 for our case (machine)
                                     // Is4 = 0 - now define, 0 for our case (machine)
        const double i41 = 0;        // const for our case (machine)
        double Xs5_dash = 0;         // == Xc_dash

        // added 21.03.2017
        double[] Xs2_doubledash = new double[dimension];
        double[] Ys2_doubledash = new double[dimension];
        
        // 2.3
        double[] A_d_i = new double[dimension];
        double[] fi_1_i = new double[dimension];
        double[] delta_T_i = new double[dimension];
        double[] delta_T_i_first = new double[dimension];
        double delta_T_first_ab = new double();
        double I_p_first = new double();
        double I_M = new double();
        double delta_T_first_a = new double();
        double delta_T_first_b = new double();

        // 2.4
        double[] T_first_i = new double[dimension];
        double[] omega_1_i = new double[dimension];
        double[] Epsilon_1_i = new double[dimension];

        // fill textBoxes for debugging
        private void forDebug()
        {
            textBox1.Text = "0,3457";   // l0
            textBox2.Text = "0,15";     // l1
            textBox3.Text = "0,6914";   // l3
            textBox4.Text = "0,6";      // l5 = Sd
            textBox6.Text = "13,8";     // m3  // value is changed at 19.03.2017
            textBox7.Text = "1";        // m4
            textBox8.Text = "10";       // Is2
            textBox9.Text = "9,42";     // ω1cp
            textBox10.Text = "0,05";    // δ
            textBox11.Text = "0,1";     // I0

            /*
            +--------------------------------------------------+
            |   change kilogramms by gramms                    |
            +--------------------------------------------------+*/
            // added 19.03.2017
            textBox12.Text = "3";       // m1
            textBox13.Text = "55,2";    // m5
            textBox14.Text = "0,00567"; // Is1
            textBox15.Text = "0,5541";  // Is3
            textBox16.Text = "51,429";  // ψ
            textBox17.Text = "90";      // η₁

            // added 17.05.2017
            // measure in Newtons
            textBox18.Text = "0,0"; // G2
            textBox19.Text = "300,0"; // G2
            textBox20.Text = "0,0"; // G2
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // tabPage1
            dgvTitles.RowCount = dgvInput.RowCount = 13;
            dgvTitles.ColumnCount = dgvInput.ColumnCount = 1;
            dgvTitles.ColumnHeadersVisible =
                dgvInput.ColumnHeadersVisible =
                dgvTitles.RowHeadersVisible =
                dgvInput.RowHeadersVisible = false;
            dgvTitles.Width = dgvInput.Width = 80;
            dgvTitles.Columns[0].Width = dgvInput.Columns[0].Width = 77;
            dgvTitles.DefaultCellStyle.Font = dgvInput.DefaultCellStyle.Font = new Font("Lucida Calligraphy", 12);
            for (int i = 0; i < 13; ++i)
            {
                dgvTitles.Rows[i].Cells[0].Value = "Fpc" + (i + 1) + " =";
            }
            dgvTitles.Enabled = false;
            cbDirection.SelectedIndex = 1;

            lbl1.Text = lbl2.Text = lbl3.Text = lbl4.Text = lbl5.Text = lbl6.Text = 
                lbl7.Text = lbl8.Text = lbl9.Text = lbl10.Text = lbl11.Text = 
                lbl18.Text = lbl19.Text = lbl20.Text = "\u2715";

            fillDgvInput();
            forDebug();

            // tabPage2
            //gbAnimation.Text = cbDirection.Text;
            dgvResults.ReadOnly = true;

            // tabPage3
            dgvResults.AllowUserToDeleteRows = dgvResults.AllowUserToResizeColumns =
                dgvResults.AllowUserToResizeRows = false;
            tbResTitle1.ReadOnly = tbResTitle2.ReadOnly = tbResTitle3.ReadOnly = true;

            // tabPage5
            fillDgvLegend();
        }        

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // btnFi_Click(this, e);

            if (checkInput())
            {
                fillColumnHeaderResult();

                Double.TryParse(textBox1.Text, out L0); // Lob
                Double.TryParse(textBox2.Text, out L1); // Loa
                Double.TryParse(textBox3.Text, out L3); // Lbc
                Double.TryParse(textBox4.Text, out L5); // Lbs3

                Double.TryParse(textBox5.Text, out fi_zero); // pseudo-constant
                fi_clone = fi_zero;                           // for iterations

                Double.TryParse(textBox6.Text, out m3);
                Double.TryParse(textBox7.Text, out m4);
                Double.TryParse(textBox8.Text, out Is4);

                Double.TryParse(textBox9.Text, out omega_1cp);

                Double.TryParse(textBox10.Text, out delta);

                Double.TryParse(textBox11.Text, out I_0_p);

                //added 19.03.2017
                Double.TryParse(textBox12.Text, out m1);
                Double.TryParse(textBox13.Text, out m5);
                Double.TryParse(textBox14.Text, out Is1);
                Double.TryParse(textBox15.Text, out Is3);

                Double.TryParse(textBox16.Text, out psi);
                Double.TryParse(textBox17.Text, out eta1);

                //added 19.03.2017
                Double.TryParse(textBox18.Text, out G2);
                Double.TryParse(textBox19.Text, out G3);
                Double.TryParse(textBox20.Text, out G4);

                // calculeate sign(+ or -) for omega_1 
                if (cbDirection.SelectedIndex == 1)
                {
                    sign_omega1 = 1F;
                }
                else 
                {
                    sign_omega1 = -1F;
                }

                // calculate G3
                // G3 = m3 * g;

// first loop
                int i = 0, j = 0;
                double fi_1 = 0F;
                for (fi_1 = fi_clone; i < dimension; ++i)
                {
                    if (fi_1 < 0)
                    {
                        fi_1 = fi_1 + 360;
                    }

                    if (fi_1 > 360)
                    {
                        fi_1 = fi_1 - 360;
                    }
/*
    +-----------------------------------------------------------+
    |       PART ONE (1.0)                                      |            
    +-----------------------------------------------------------+
*/

// =============== 1.1 part of calculating =================
                    try
                    {
                        fi_array[i] = fi_1;

                        Xa[i] = L1 * Math.Cos(degToRad(fi_1));
                        Ya[i] = L1 * Math.Sin(degToRad(fi_1));

                        L[i] = Math.Sqrt(Math.Pow(L0, 2) + Math.Pow(L1, 2) + 2 * L0 * L1 * Math.Sin(degToRad(fi_1)));

                        cos_fi3[i] = (L1 * Math.Cos(degToRad(fi_1))) / L[i];
                        sin_fi3[i] = (L0 + L1 * Math.Sin(degToRad(fi_1))) / L[i];

                        fi3[i] = radToDeg(Math.Acos((L1 * Math.Cos(degToRad(fi_1))) / L[i]));

                        Xc[i] = L3 * cos_fi3[i];
                        Yc[i] = L3 * sin_fi3[i];
                        Xs3[i] = L5 * cos_fi3[i];
                        Ys3[i] = L5 * sin_fi3[i];
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 1.1"); }

// =============== 1.2 part of calculating =================
                    try
                    {
                        Xa_dash[i] = -L1 * Math.Sin(degToRad(fi_1));
                        Ya_dash[i] = L1 * Math.Cos(degToRad(fi_1));

                        Ua3a2[i] = -L1 * Math.Sin(degToRad(fi_1 - fi3[i]));
                        i31[i] = (L1 / L[i]) * Math.Cos(degToRad(fi_1 - fi3[i]));

                        Sd[i] = Xc[i] - L3 * cos_fi3[0];
                        
                        Xc_dash[i] = (-i31[i]) * L3 * sin_fi3[i];
                        Yc_dash[i] = i31[i] * L3 * cos_fi3[i];
                                                
                        Uc[i] = i31[i] * L3;

                        Xs3_dash[i] = (-i31[i]) * L5 * sin_fi3[i];
                        Ys3_dash[i] = i31[i] * L5 * cos_fi3[i];

                        Us3[i] = i31[i] * L5;

                        Us5[i] = Xc_dash[i];

                        i31_dash[i] = (-Ua3a2[i]) * (2 * i31[i] - 1) / L[i];
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 1.2.1"); }

                    try
                    {
                        // added 06.03.2017
                        Xc_doubledash[i] = (-L3) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                        Yc_doubledash[i] = (-L3) * (Math.Pow(i31[i],2) * sin_fi3[i] + i31_dash[i] * cos_fi3[i]);
                    }
                    catch (Exception exe) { MessageBox.Show(exe.Data + "\n" + exe.Message + "\nОшибка в части 1.2.2"); }

// =============== 1.3 (added 06.03.2017) part of calculating =================
                    try
                    {
                        // added 06.03.2017
                        Xs3_doubledash[i] = (-L5) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                        Ys3_doubledash[i] = L5 * (i31_dash[i] * cos_fi3[i] - Math.Pow(i31[i], 2) * sin_fi3[i]);
                        is51_dash[i] = (-L3) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 1.3"); }

/*
    +-----------------------------------------------------------+
    |                PART TWO (2.0) added 16.03.2017            |            
    +-----------------------------------------------------------+
*/

// =============== 2.1 part of calculating =================
                    try
                    {
                        // preparing calculations
                        Xd_dash[i] = Xc_dash[i];
                        Ys2_dash[i] = Ya_dash[i];
                        Ys4_dash[i] = Yc_dash[i];

                        M_c_pi[i] = sign_omega1 * (-Fpc[i] * Xd_dash[i] - G2 * Ys2_dash[i] - G3 * Ys3_dash[i] - G4 * Ys4_dash[i]);

                        double delta_fi1 = /*sign_omega1 * */2 * Math.PI / 180F;
                        if (i == 0)
                        {
                            A_c_i[i] = - 0.5 * M_c_pi[i] * delta_fi1;
                        }
                        else
                        {
                            A_c_i[i] = A_c_i[i - 1] - delta_fi1 * 0.5 * (M_c_pi[i - 1] + M_c_pi[i]);
                        }                            
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 2.1"); }
                    
                    if (cbDirection.SelectedIndex == 0)
                        fi_1 -= delta_fi;
                    else
                        fi_1 += delta_fi;
                }

                try
                {
                    M_p_D = -(A_c_i[180] / (2F * Math.PI));
                }
                catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в вычислении M_p_D."); }

// second loop
                i = 0; 
                j = 0;
                fi_1 = 0F;
                for (fi_1 = fi_clone; i < dimension; ++i)
                {
                    if (fi_1 < 0)
                    {
                        fi_1 = fi_1 + 360;
                    }

                    if (fi_1 > 360)
                    {
                        fi_1 = fi_1 - 360;
                    }

                    // =============== 2.2 part of calculating =================
                    try
                    {
                        Xs2_dash = Xa_dash[i];
                        Ys2_dash = Ya_dash;
                        // Is2 = 0 - const for our case (machine)
                        i21 = i31[i];
                        // m2 = 0 - const for our case (machine)
                        // Xs4_dash = 0 - const for our case (machine)
                        // Ys4_dash - now define
                        // m4 = 0 - now define, 0 for our case (machine)
                        // Is4 = 0 - now define, 0 for our case (machine)
                        // i41 = 0 - const for our case (machine)
                        Xs5_dash = Xc_dash[i];

                        // A, B, C, D, E, F, G - temp variables for calculating the I_pa_second
                        // A, B - second point
                        A = m2 * (Math.Pow(Xs2_dash, 2) + Math.Pow(Ys2_dash[i], 2));
                        B = Is2 * Math.Pow(i21, 2);
                        // C, D - third point
                        C = m3 * (Math.Pow(Xs3_dash[i], 2) + Math.Pow(Ys3_dash[i], 2));
                        D = Is3 * Math.Pow(i31[i], 2);
                        // E, F - fourth point
                        E = m4 * (Math.Pow(Xs4_dash, 2) + Math.Pow(Ys4_dash[i], 2));
                        F = Is4 * Math.Pow(i41, 2);
                        // G - fifth point
                        G = m5 * Math.Pow(Xs5_dash, 2);

                        // calculating final value of I_pa_second[i]
                        I_pa_second[i] = A + B + C + D + E + F + G;

                        // not initialized
                        double[] Xs2_doubledash = new double[dimension];
                        double[] i21_dash = new double[dimension];
                        double[] Xs4_doubledash = new double[dimension];
                        double[] Ys4_doubledash = new double[dimension];
                        double[] i41_dash = new double[dimension];
                        double[] Xs5_doubledash = new double[dimension];

                        differential_d_Ip_d_fi1[i] = 2 * (
                              m2 * (Xs2_dash * Xs2_doubledash[i] + Ys2_dash[i] * Ys2_doubledash[i])
                            + Is2 * i21 * i21_dash[i]
                            + m3 * (Xs3_dash[i] * Xs3_doubledash[i] + Ys3_dash[i] * Ys3_doubledash[i])
                            + Is3 * i31[i] * i31_dash[i]
                            + m4 * (Xs4_dash * Xs4_doubledash[i] + Ys4_dash[i] * Ys4_doubledash[i]
                            + Is4 * i41 * i41_dash[i]
                            + m5 * (Xs5_dash * Xs5_doubledash[i]))
                            );
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\n\nОшибка в части 2.2"); }

                    try
                    {
                        fi_1_i[i] = Math.Abs(degToRad(delta_fi)) * (i - 1);
                        A_d_i[i] = M_p_D * fi_1_i[i];

                        delta_T_i[i] = A_d_i[i] + A_c_i[i];
                        /*
                        +-------------------------------------------+
                        |   maybe I_p_second[i] = I_pa_second[i]    |
                        +-------------------------------------------+*/
                        double delta_T_i_second = (I_pa_second[i] * Math.Pow(omega_1cp, 2)) / 2;
                        delta_T_i_first[i] = delta_T_i[i] - delta_T_i_second;
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\n\nОшибка в начале части 2.3"); }

                    if (cbDirection.SelectedIndex == 0)
                        fi_1 -= delta_fi;
                    else
                        fi_1 += delta_fi;
                }
// =============== 2.3 part of calculating =================  
                try
                {
                    // search MAX and MIN in delta_T_i_first[]
                    delta_T_first_a = delta_T_i_first.Max();
                    delta_T_first_b = delta_T_i_first.Min();
                    delta_T_first_ab = delta_T_first_a - delta_T_first_b;

                    I_p_first = delta_T_first_ab / (delta * Math.Pow(omega_1cp, 2));

                    I_M = I_p_first - I_0_p;
                }
                catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 2.3"); }

// third loop
                i = 0; 
                j = 0;
                fi_1 = 0F;
                for (fi_1 = fi_clone; i < dimension; ++i)
                {
                    if (fi_1 < 0)
                    {
                        fi_1 = fi_1 + 360;
                    }

                    if (fi_1 > 360)
                    {
                        fi_1 = fi_1 - 360;
                    }

// =============== 2.4 part of calculating =================
                    try
                    {
                        double T_first_cp = (I_p_first * Math.Pow(omega_1cp, 2)) / 2;
                        double delta_T1_cp = (delta_T_first_a + delta_T_first_b) / 2;
                        T_first_i[i] = T_first_cp - delta_T1_cp + delta_T_i_first[i];
                        omega_1_i[i] = sign_omega1 * Math.Sqrt((2 * T_first_i[i]) / I_p_first);
                        Epsilon_1_i[i] = sign_omega1 
                            * (M_p_D - M_c_pi[i] - (Math.Pow(omega_1_i[i], 2) / 2) * differential_d_Ip_d_fi1[i])
                            / (I_p_first + I_pa_second[i]);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message + "\nОшибка в части 2.4"); }

/*
+-------------------------------------------+
|   output values into dgvResult            |
+-------------------------------------------+*/

                    if ((i % 15 == 0) || (i == 0))
                    {
                        dgvResults.Rows[j].HeaderCell.Value = String.Format("{0:0.#####}", j + 1);
                        dgvResults.Rows[j].Cells[0].Value = String.Format("{0:0.#####}", fi_1);
                        dgvResults.Rows[j].Cells[1].Value = String.Format("{0:0.#####}", Sd[i]);
                        dgvResults.Rows[j].Cells[2].Value = String.Format("{0:0.#####}", i31[i]);
                        dgvResults.Rows[j].Cells[3].Value = String.Format("{0:0.#####}", i31_dash[i]);
                        dgvResults.Rows[j].Cells[4].Value = String.Format("{0:0.#####}", Xs3_dash[i]);
                        dgvResults.Rows[j].Cells[5].Value = String.Format("{0:0.#####}", Ys3_dash[i]);
                        dgvResults.Rows[j].Cells[6].Value = String.Format("{0:0.#####}", Xs3_doubledash[i]);
                        dgvResults.Rows[j].Cells[7].Value = String.Format("{0:0.#####}", Ys3_doubledash[i]);
                        dgvResults.Rows[j].Cells[8].Value = String.Format("{0:0.#####}", Xc_dash[i]);
                        dgvResults.Rows[j].Cells[9].Value = String.Format("{0:0.#####}", I_pa_second[i]);
                        dgvResults.Rows[j].Cells[10].Value = String.Format("{0:0.#####}", differential_d_Ip_d_fi1[i]);
                        dgvResults.Rows[j].Cells[11].Value = String.Format("{0:0.#####}", Xc[i]);
                        dgvResults.Rows[j].Cells[12].Value = String.Format("{0:0.#####}", Xc_doubledash[i]);
                        dgvResults.Rows[j].Cells[13].Value = String.Format("{0:0.#####}", omega_1_i[i]);
                        dgvResults.Rows[j].Cells[14].Value = String.Format("{0:0.#####}", Epsilon_1_i[i]);

                        ++j;
                    }

                    if (cbDirection.SelectedIndex == 0)
                        fi_1 -= delta_fi;
                    else
                        fi_1 += delta_fi;
                }

                accessFlag = true;
                tabControl1.SelectedIndex = 1;

                // run animation
                //this.gbAnimation.Paint += new PaintEventHandler(TabPage_Paint);

                // output three results into textBoxes
                tbResValue1.Text = string.Format("{0:0.#####}", M_p_D);
                tbResValue2.Text = string.Format("{0:0.#####}", I_p_first);
                tbResValue3.Text = string.Format("{0:0.#####}", I_M);
            }
            else
            {
                MessageBox.Show("Заполните все поля с исходными данными!");
            }
        }

        private void легендаОбозначенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

/*=================================================================================================== 
 *=== Build charts  
 *===================================================================================================*/

        private void btnChart_sd_i31_i31P_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("Sd").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("i31").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("i31'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Sd"].Points.AddXY(i, Sd[i]);
                chart1.Series["i31"].Points.AddXY(i, i31[i]);
                chart1.Series["i31'"].Points.AddXY(i, i31_dash[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(Math.Min(Sd.Min(), i31.Min()), i31_dash.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(Math.Max(Sd.Max(), i31.Max()), i31_dash.Max()));
            groupBox8.Text = btnChart_sd_i51_i51P.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_i21_i21P_Click(object sender, EventArgs e)
        {            
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("i21").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("i21'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["i21"].Points.AddXY(i, i31[i]); // i21 == i31 => i21p == i31p
                chart1.Series["i21'"].Points.AddXY(i, i31_dash[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(i31.Min(), i31_dash.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(i31.Max(), i31_dash.Max()));            
            groupBox8.Text = btnChart_i21_i21P.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_xs2p_ys2p_xs2pp_ys2pp_Click(object sender, EventArgs e)
        {            
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("Xs3p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys3p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xs3pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys3pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Xs3p"].Points.AddXY(i, Xs3_dash[i]);
                chart1.Series["Ys3p"].Points.AddXY(i, Ys3_dash[i]);
                chart1.Series["Xs3pp"].Points.AddXY(i, Xs3_doubledash[i]);
                chart1.Series["Ys3pp"].Points.AddXY(i, Ys3_doubledash[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(Math.Min(Xs3_dash.Min(), Ys3_dash.Min()), Math.Min(Xs3_doubledash.Min(), Ys3_doubledash.Min())));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(Math.Max(Xs3_dash.Max(), Ys3_dash.Max()), Math.Max(Xs3_doubledash.Max(), Ys3_doubledash.Max())));
            groupBox8.Text = btnChart_xs2p_ys2p_xs2pp_ys2pp.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_Mcp_Mdp_Click(object sender, EventArgs e)
        {            
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("Mpc").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ac").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Mpc"].Points.AddXY(i, M_c_pi[i]);
                chart1.Series["Ac"].Points.AddXY(i, A_c_i[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(M_c_pi.Min(), A_c_i.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(M_c_pi.Max(), A_c_i.Max()));
            groupBox8.Text = btnChart_Mcp_Mdp.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_dT_dTi_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("delta Ti").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("delta Ti'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["delta Ti"].Points.AddXY(i, delta_T_i[i]);
                chart1.Series["delta Ti'"].Points.AddXY(i, delta_T_i_first[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(delta_T_i.Min(), delta_T_i_first.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(delta_T_i.Max(), delta_T_i_first.Max()));
            groupBox8.Text = btnChart_dT_dTi.Text;   
            tabControl1.SelectedIndex = 2;
        }
        
        private void btnChart_omega1_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("ω1").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["ω1"].Points.AddXY(i, omega_1_i[i]);                 
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(omega_1_i.Min());
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(omega_1_i.Max());            
            groupBox8.Text = btnChart_omega1.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_Ac_Ad_Click(object sender, EventArgs e) // must repair
        {            
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("Ac").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ad").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Ac"].Points.AddXY(i, A_c_i[i]);
                chart1.Series["Ad"].Points.AddXY(i, A_d_i[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(A_c_i.Min(), A_d_i.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(A_c_i.Max(), A_d_i.Max()));
            groupBox8.Text = btnChart_Ac_Ad.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btnChart_xc_dash_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("Xc").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xc'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xc''").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Xc"].Points.AddXY(i, Xc[i]);
                chart1.Series["Xc'"].Points.AddXY(i, Xc_dash[i]);
                chart1.Series["Xc''"].Points.AddXY(i, Xc_doubledash[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(Math.Min(Xc.Min(), Xc_dash.Min()), Xc_doubledash.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(Math.Max(Xc.Max(), Xc_dash.Max()), Xc_doubledash.Max()));
            groupBox8.Text = btnChart_xc_dash.Text;
            tabControl1.SelectedIndex = 2;
        }

        private void btn_I_pa_second_d_Yp_d_fi1_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear(); 
            chart1.ChartAreas[0].AxisY.RoundAxisValues();
            chart1.Series.Add("I_pa_second").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Yп_d_fi1'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["I_pa_second"].Points.AddXY(i, I_pa_second[i]);
                chart1.Series["Yп_d_fi1'"].Points.AddXY(i, differential_d_Ip_d_fi1[i]);
            }
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(Math.Min(I_pa_second.Min(), differential_d_Ip_d_fi1.Min()));
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(Math.Max(I_pa_second.Max(), differential_d_Ip_d_fi1.Max()));
            groupBox8.Text = btnChart_I_pa_second_d_Yp_d_fi1.Text;
            tabControl1.SelectedIndex = 2;
        }

/*=================================================================================================== 
 *=== Validate input data  
 *===================================================================================================*/

        private Boolean checkInput()
        {
            if (lbl1.Text == "\u2714" && lbl2.Text == "\u2714" && lbl3.Text == "\u2714"
                && lbl4.Text == "\u2714" && lbl5.Text == "\u2714" && lbl6.Text == "\u2714"
                && lbl7.Text == "\u2714" && lbl8.Text == "\u2714" && lbl9.Text == "\u2714"
                && lbl10.Text == "\u2714" && lbl11.Text == "\u2714" 
                // added at 19.03.2017
                && lbl12.Text == "\u2714" && lbl13.Text == "\u2714" && lbl14.Text == "\u2714"
                && lbl15.Text == "\u2714" && lbl16.Text == "\u2714" && lbl17.Text == "\u2714"
                // added at 17.05.2017
                && lbl18.Text == "\u2714" && lbl19.Text == "\u2714" && lbl20.Text == "\u2714")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void dgvInput_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // must repair filling array Fpc;
            double temp = 0;
            int Fpc_index = 0;
            for (int i = 0; i < dimension; ++i)
            {
                Fpc_index = i / (dimension / dgvInput.RowCount + 2);
                if ((Convert.ToString(dgvInput.Rows[Fpc_index].Cells[0].Value) != ""))
                    if (Double.TryParse(Convert.ToString(dgvInput.Rows[Fpc_index].Cells[0].Value), out temp))
                    {
                        Fpc[i] = Convert.ToDouble(dgvInput.Rows[Fpc_index].Cells[0].Value);
                    }
                    else
                    {
                        dgvInput.Rows[Fpc_index].Cells[0].Value = Convert.ToString(0);
                        Fpc[i] = 0;
                        MessageBox.Show("Допускаются лишь следующие символы: {0-9}, {,}.\n"
                            + "Пустая ячейка не допускается.");
                    }
            }
        } 

        private void btnFi_Click(object sender, EventArgs e)
        {
            double OB = 0;
            double OA = 0;
            if (Double.TryParse(textBox2.Text, out OA) && Double.TryParse(textBox1.Text, out OB))
            {
                double fi = 270 - Math.Acos(OA / OB) * (180 / Math.PI);
                if (Double.IsNaN(fi) || Double.IsInfinity(fi))
                {
                    throw new ArgumentException("Угол φ не найден.\nПрограмма обнуляет значения.\nПопробуйте снова.");
                }
                else
                {
                    textBox5.Text = String.Format("{0:0.##########}", fi); // ten characters before point
                    lbl5.Text = "\u2714";
                }
            }
        }

        private void tpAnimation_Enter(object sender, EventArgs e)
        {
            if (!accessFlag)
            {
                tabControl1.SelectedTab = tpInput;
                MessageBox.Show("Вычисления не выполнены. \n"
                    + "Для доступа к \n\t- анимации \n\t- результатам вычислений \n\t- графикам \nвведите все исходные данные и нажмите кнопку \"Выполнить вычисления\"");
            }
            else 
            {
                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = 180;
            }
        }

        private void validateInput(TextBox tb, Label lbl)
        {
            double temp = 0;
            if (Double.TryParse(tb.Text, out temp) && tb.Text.Length != 0)
                lbl.Text = "\u2714";
            else
                lbl.Text = "\u2715";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox1, lbl1);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox2, lbl2);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox3, lbl3);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox4, lbl4);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox5, lbl5);
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox6, lbl6);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox7, lbl7);
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox8, lbl8);
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox9, lbl9);
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox10, lbl10);
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox11, lbl11);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar == ',' || e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }

        // added 19.03.2017
        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox12, lbl12);
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox13, lbl13);
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox14, lbl14);
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox15, lbl15);
        }

        private void textBox16_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox16, lbl16);
        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox17, lbl17);
        }

        // added 17.05.2017
        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox18, lbl18);
        }
        
        private void textBox19_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox19, lbl19);
        }

        private void textBox20_TextChanged(object sender, EventArgs e)
        {
            validateInput(textBox20, lbl20);
        }

/*=================================================================================================== 
 *=== filling some fields  
 *===================================================================================================*/

        private void fillColumnHeaderResult()
        {
            dgvResults.ColumnCount = 15;
            dgvResults.RowCount = 13;
            dgvResults.RowHeadersWidth = 90;
            for (int i = 0; i < dgvResults.ColumnCount; ++i)
            {
                dgvResults.Columns[i].Width = 60;
            }

            dgvResults.Columns[0].HeaderText = "φo";
            dgvResults.Columns[1].HeaderCell.Value = "Sd";
            dgvResults.Columns[2].HeaderCell.Value = "i31";
            dgvResults.Columns[3].HeaderCell.Value = "i31p";
            dgvResults.Columns[4].HeaderCell.Value = "X3p";
            dgvResults.Columns[5].HeaderCell.Value = "Y3p";
            dgvResults.Columns[6].HeaderCell.Value = "X3pp";
            dgvResults.Columns[7].HeaderCell.Value = "Y3pp";
            dgvResults.Columns[8].HeaderCell.Value = "Xc'";
            dgvResults.Columns[9].HeaderCell.Value = "Ipᴵᴵ";
            dgvResults.Columns[10].HeaderCell.Value = "dIп/dφ1";
            dgvResults.Columns[11].HeaderCell.Value = "Xc";
            dgvResults.Columns[12].HeaderCell.Value = "Xc''";
            dgvResults.Columns[13].HeaderCell.Value = "ω1";
            dgvResults.Columns[14].HeaderCell.Value = "ε1";
        }

        private void fillDgvInput()
        {
            for (int i = 0; i <= 12; ++i)
            {
                int flag = 1;
                if (cbDirection.SelectedIndex == 0)
                {
                    flag = -1;
                }
                else
                {
                    flag = 1;
                }
                if (i > 4 && i < 11)
                {
                    Fpc[i] =  flag * 5000F;
                    dgvInput.Rows[i].Cells[0].Value = Fpc[i].ToString();
                }
                else
                {
                    Fpc[i] = flag * 100F;
                    dgvInput.Rows[i].Cells[0].Value = Fpc[i].ToString();
                }
            }
        }

/*=================================================================================================== 
 *=== fill dgvLegend 
 *===================================================================================================*/

        private void fillDgvLegend()
        {
            // ₁ ₂ ₃ ₄ ₅ φ ′ ᴵ Δ ε ω ᶜ
            dgvLegend.Font = new Font("Consolas", 12);
            dgvLegend.RowCount = 33;

            dgvLegend.Rows[0].Cells[0].Value = "Кинематические характеристики исполнительного механизма";

            dgvLegend.Rows[1].Cells[0].Value = "Обобщённая координата";
            dgvLegend.Rows[1].Cells[1].Value = "φ₁";
            dgvLegend.Rows[1].Cells[2].Value = "Fi";

            dgvLegend.Rows[2].Cells[0].Value = "Перемещение ползуна";
            dgvLegend.Rows[2].Cells[1].Value = "S5";
            dgvLegend.Rows[2].Cells[2].Value = "S5";

            dgvLegend.Rows[3].Cells[0].Value = "Передаточная функция кулисы";
            dgvLegend.Rows[3].Cells[1].Value = "i₃₁";
            dgvLegend.Rows[3].Cells[2].Value = "i31";

            dgvLegend.Rows[4].Cells[0].Value = "Передаточная функция шатуна";
            dgvLegend.Rows[4].Cells[1].Value = "i₄₁";
            dgvLegend.Rows[4].Cells[2].Value = "i41";

            dgvLegend.Rows[5].Cells[0].Value = "Передаточная функция ползуна";
            dgvLegend.Rows[5].Cells[1].Value = "i₅₁";
            dgvLegend.Rows[5].Cells[2].Value = "i51";

            dgvLegend.Rows[6].Cells[0].Value = "Проекция аналога скорости точки S₃ на ось x";
            dgvLegend.Rows[6].Cells[1].Value = "X′s₃";
            dgvLegend.Rows[6].Cells[2].Value = "xS3P";

            dgvLegend.Rows[7].Cells[0].Value = "Проекция аналога скорости точки S₃ на ось y";
            dgvLegend.Rows[7].Cells[1].Value = "Y′s₃";
            dgvLegend.Rows[7].Cells[2].Value = "yS3P";

            dgvLegend.Rows[8].Cells[0].Value = "Проекция аналога скорости точки S₄ на ось x";
            dgvLegend.Rows[8].Cells[1].Value = "X′s₄";
            dgvLegend.Rows[8].Cells[2].Value = "yS4P";

            dgvLegend.Rows[9].Cells[0].Value = "Проекция аналога скорости точки S₄ на ось y";
            dgvLegend.Rows[9].Cells[1].Value = "Y′s₄";
            dgvLegend.Rows[9].Cells[2].Value = "yS4P";

            dgvLegend.Rows[10].Cells[0].Value = "Производная передаточной функции кулисы";
            dgvLegend.Rows[10].Cells[1].Value = "i′₃₁";
            dgvLegend.Rows[10].Cells[2].Value = "i31P";

            /* must change indexes */

            dgvLegend.Rows[11].Cells[0].Value = "Производная передаточной функции звена 4";
            dgvLegend.Rows[11].Cells[1].Value = "i′₄₁";
            dgvLegend.Rows[11].Cells[2].Value = "i41P";

            dgvLegend.Rows[12].Cells[0].Value = "Производная передаточной функции ползуна";
            dgvLegend.Rows[12].Cells[1].Value = "i′₅₁";
            dgvLegend.Rows[12].Cells[2].Value = "i51P";

            dgvLegend.Rows[13].Cells[0].Value = "Проекция аналога ускорения точки S₂ на ось x";
            dgvLegend.Rows[13].Cells[1].Value = "X′′s₂";
            dgvLegend.Rows[13].Cells[2].Value = "yS2PP";

            dgvLegend.Rows[14].Cells[0].Value = "Проекция аналога ускорения точки S₂ на ось y";
            dgvLegend.Rows[14].Cells[1].Value = "Y′′s₂";
            dgvLegend.Rows[14].Cells[2].Value = "yS2PP";

            dgvLegend.Rows[15].Cells[0].Value = "Переменная составляющая приведенного момента инерции";

            dgvLegend.Rows[16].Cells[0].Value = "Часть от массы камня кулисы";
            dgvLegend.Rows[16].Cells[1].Value = "A";
            dgvLegend.Rows[16].Cells[2].Value = "A";

            dgvLegend.Rows[17].Cells[0].Value = "Часть от Iпᴵᴵ камня кулисы";
            dgvLegend.Rows[17].Cells[1].Value = "B";
            dgvLegend.Rows[17].Cells[2].Value = "B";

            dgvLegend.Rows[18].Cells[0].Value = "Часть от массы кулисы";
            dgvLegend.Rows[18].Cells[1].Value = "C";
            dgvLegend.Rows[18].Cells[2].Value = "C";

            dgvLegend.Rows[19].Cells[0].Value = "Часть от Iпᴵᴵ кулисы";
            dgvLegend.Rows[19].Cells[1].Value = "D";
            dgvLegend.Rows[19].Cells[2].Value = "D";

            dgvLegend.Rows[20].Cells[0].Value = "Часть массы звена 4";
            dgvLegend.Rows[20].Cells[1].Value = "E";
            dgvLegend.Rows[20].Cells[2].Value = "E";

            dgvLegend.Rows[21].Cells[0].Value = "Часть от Iпᴵᴵ звена 4";
            dgvLegend.Rows[21].Cells[1].Value = "F";
            dgvLegend.Rows[21].Cells[2].Value = "F";

            dgvLegend.Rows[22].Cells[0].Value = "Часть от массы ползуна";
            dgvLegend.Rows[22].Cells[1].Value = "G";
            dgvLegend.Rows[22].Cells[2].Value = "G";

            dgvLegend.Rows[23].Cells[0].Value = "Переменная составляющая приведенного момента инерции";
            dgvLegend.Rows[23].Cells[1].Value = "Iпᴵᴵ";
            dgvLegend.Rows[23].Cells[2].Value = "I2p";

            dgvLegend.Rows[24].Cells[0].Value = "Производная произведённого момента инерции";
            dgvLegend.Rows[24].Cells[1].Value = "dIп / dφ₁";
            dgvLegend.Rows[24].Cells[2].Value = "dIpdFi";

            dgvLegend.Rows[25].Cells[0].Value = "Определение закона движения звена произведения";

            dgvLegend.Rows[26].Cells[0].Value = "Приведённый момент сил сопротивления";
            dgvLegend.Rows[26].Cells[1].Value = "Mпᶜ";
            dgvLegend.Rows[26].Cells[2].Value = "Mpс";

            dgvLegend.Rows[27].Cells[0].Value = "Работа сил сопротивления";
            dgvLegend.Rows[27].Cells[1].Value = "Ac";
            dgvLegend.Rows[27].Cells[2].Value = "Ac";

            dgvLegend.Rows[28].Cells[0].Value = "Работа движущих сил";
            dgvLegend.Rows[28].Cells[1].Value = "Aд";
            dgvLegend.Rows[28].Cells[2].Value = "Ad";

            dgvLegend.Rows[29].Cells[0].Value = "Изменение кинеической энергии машины";
            dgvLegend.Rows[29].Cells[1].Value = "ΔT";
            dgvLegend.Rows[29].Cells[2].Value = "dT";

            dgvLegend.Rows[30].Cells[0].Value = "Изменение кинеической энергии пост. сост-ей прив-го момента инерции";
            dgvLegend.Rows[30].Cells[1].Value = "ΔTɪ";
            dgvLegend.Rows[30].Cells[2].Value = "dTI";

            dgvLegend.Rows[31].Cells[0].Value = "Угловая скорость кривошипа";
            dgvLegend.Rows[31].Cells[1].Value = "ω₁";
            dgvLegend.Rows[31].Cells[2].Value = "w1";

            dgvLegend.Rows[32].Cells[0].Value = "Угловое ускорение кривошипа";
            dgvLegend.Rows[32].Cells[1].Value = "ε₁";
            dgvLegend.Rows[32].Cells[2].Value = "e1";
        }

/*=================================================================================================== 
 *=== some math  
 *===================================================================================================*/

        public double degToRad(double param)
        {
            return (param * Math.PI / 180F);
        }

        public double radToDeg(double param)
        {
            return (param * 180F / Math.PI);
        }

/*=================================================================================================== 
 *=== for animations  
 *===================================================================================================*/

        // start animate
        private void btnStart_Click(object sender, EventArgs e)
        {

        }

        // pause animate
        private void btnStop_Click(object sender, EventArgs e)
        {

        }
        /*
        protected void TabPage_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen arrow = new Pen(Brushes.Black, 1);
            arrow.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            int scale = 400;
            int scale_L0_OB = (int)(L0 * (double)scale);
            int scale_L1_OA = (int)(L1 * (double)scale);
            int scale_L3_BC = (int)(L3 * (double)scale);
            int scale_L3_BS3 = (int)(L5 * (double)scale);

            int x_mid = gbAnimation.Width / 2;
            int x_left = 20;
            int x_right = gbAnimation.Width - 20;
            int y_top = 20;
            int y_bottom = gbAnimation.Height - 20;

            int fix_x_bottom = x_mid;
            int fix_y_bottom = y_bottom - 10;

            int fix_x_mid = x_mid;
            int fix_y_mid = y_bottom - 10 - scale_L0_OB;


            // draw bottom fix point
            e.Graphics.DrawLine(arrow, x_mid - 15, y_bottom + 10, x_mid + 15, y_bottom + 10);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10, x_mid - 9, y_bottom + 10);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10, x_mid + 9, y_bottom + 10);
            e.Graphics.DrawEllipse(arrow, x_mid - 3, y_bottom - 10 - 3, 6, 6);
            for (int i = (x_mid - 15); i <= (x_mid + 15 - 5); i = i + 5)
                e.Graphics.DrawLine(arrow, i, y_bottom + 15, 5 + i, y_bottom + 10);

            // draw fixed point for top slider

            // left top line
            e.Graphics.DrawLine(arrow, x_left + 200, y_top + 10, x_left + 215, y_top + 10);
            // left bottom line
            e.Graphics.DrawLine(arrow, x_left + 200, y_top + 20, x_left + 215, y_top + 20);
            // left top dash
            for (int i = (x_left + 200); i <= (x_left + 215 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, i, y_top + 10, 4 + i, y_top + 5);
            // left bottom dash
            for (int i = (x_left + 200); i <= (x_left + 215 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, 4 + i, y_top + 20, i, y_top + 25);

            // right top line
            e.Graphics.DrawLine(arrow, x_right - 200, y_top + 10, x_right - 215, y_top + 10);
            // right bottom line
            e.Graphics.DrawLine(arrow, x_right - 200, y_top + 20, x_right - 215, y_top + 20);
            // right top dash
            for (int i = (x_right - 215); i <= (x_right - 200 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, i, y_top + 10, 4 + i, y_top + 5);
            // right bottom dash
            for (int i = (x_right - 215); i <= (x_right - 200 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, 4 + i, y_top + 20, i, y_top + 25);

            // draw top slider
            e.Graphics.DrawLine(arrow, x_left + 50, y_top + 15, x_right - 50, y_top + 15);

            // draw mid fix point
            e.Graphics.DrawLine(arrow, x_mid - 15, y_bottom + 10 - scale_L0_OB, x_mid + 15, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10 - scale_L0_OB, x_mid - 9, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10 - scale_L0_OB, x_mid + 9, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawEllipse(arrow, x_mid - 3, y_bottom - 10 - 3 - scale_L0_OB, 6, 6);
            for (int i = (x_mid - 15); i <= (x_mid + 15 - 5); i = i + 5)
                e.Graphics.DrawLine(arrow, i, y_bottom + 15 - scale_L0_OB, 5 + i, y_bottom + 10 - scale_L0_OB);

            // draw bottom rocker
            e.Graphics.DrawLine(arrow, fix_x_bottom, fix_y_bottom, fix_x_bottom, fix_y_bottom - scale_L3_BC);

            // draw mid rocker
            e.Graphics.DrawLine(arrow, fix_x_mid, fix_y_mid, fix_x_mid + scale_L1_OA, fix_y_mid);

            // draw mid 'поршень'
            e.Graphics.DrawRectangle(arrow, fix_x_mid + scale_L1_OA, fix_y_mid, 10, 20);

            //Thread.Sleep(500);
            arrow.Dispose();
        }
    */

/*=================================================================================================== 
 *=== methods for printing  
 *===================================================================================================*/

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                CaptureScreen();
                printDocument.Print();
            }
            catch (Exception) 
            { 
                MessageBox.Show(
                    "У вас нет доступных принтеров.",
                    "Ошибка подключения к принтеру",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification); 
            }
        }

        private void CaptureScreen()
        {
            Graphics myGraphics = this.CreateGraphics();
            Size s = this.Size;
            Bitmap memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);
        }

/*=================================================================================================== 
 *=== output reports 
 *===================================================================================================*/

        ReportForm f = new ReportForm();

        // Кинематические характеристики исполнительного механизма
        private void button8_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;

            // for DataGridView
            this.Text = button8.Text;
            dgv.RowCount = dgvResults.RowCount;
            dgv.ColumnCount = 3;
            dgv.DataSource = dgvResults.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgv.Rows[i].Cells[0].Value = dgvResults.Rows[i].Cells[11].Value;
                dgv.Rows[i].Cells[1].Value = dgvResults.Rows[i].Cells[8].Value;
                dgv.Rows[i].Cells[2].Value = dgvResults.Rows[i].Cells[12].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgv.ColumnCount; ++i)
            {
                dgv.Columns[i].Width = 55;
            }
            string title_first = (string)dgvResults.Columns[11].HeaderCell.Value;
            string title_second = (string)dgvResults.Columns[8].HeaderCell.Value;
            string title_third = (string)dgvResults.Columns[12].HeaderCell.Value;
            dgv.Columns[0].HeaderCell.Value = title_first;
            dgv.Columns[1].HeaderCell.Value = title_second;
            dgv.Columns[2].HeaderCell.Value = title_third;
            dgv.RowHeadersVisible = false;

            // for Chart
            chart2.Series.Dispose();
            chart2.Series.Clear();
            chart2.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart2.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart2.Series.Add(title_third).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart2.Series[title_first].Points.AddXY(i, Xc[i]);
                chart2.Series[title_second].Points.AddXY(i, Xc_dash[i]);
                chart2.Series[title_third].Points.AddXY(i, Xc_doubledash[i]);
            }
        }

        // Переменная составляющая приведённого момента инерции
        private void button7_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;

            // for DataGridView
            this.Text = button7.Text;
            dgv.RowCount = dgvResults.RowCount;
            dgv.ColumnCount = 2;
            dgv.DataSource = dgvResults.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgv.Rows[i].Cells[0].Value = dgvResults.Rows[i].Cells[9].Value;
                dgv.Rows[i].Cells[1].Value = dgvResults.Rows[i].Cells[10].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgv.ColumnCount; ++i)
            {
                dgv.Columns[i].Width = 55;
            }
            string title_first = (string)dgvResults.Columns[9].HeaderCell.Value;
            string title_second = (string)dgvResults.Columns[10].HeaderCell.Value;
            dgv.Columns[0].HeaderCell.Value = title_first;
            dgv.Columns[1].HeaderCell.Value = title_second;
            dgv.RowHeadersVisible = false;

            // for Chart
            chart2.Series.Dispose();
            chart2.Series.Clear();
            chart2.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart2.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart2.Series[title_first].Points.AddXY(i, I_pa_second[i]);
                chart2.Series[title_second].Points.AddXY(i, differential_d_Ip_d_fi1[i]);
            }
        }

        // Определение закона инерции        
        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
            
            // for DataGridView
            this.Text = button6.Text;
            dgv.RowCount = dgvResults.RowCount;
            dgv.ColumnCount = 2;
            dgv.DataSource = dgvResults.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgv.Rows[i].Cells[0].Value = dgvResults.Rows[i].Cells[13].Value;
                dgv.Rows[i].Cells[1].Value = dgvResults.Rows[i].Cells[14].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgv.ColumnCount; ++i)
            {
                dgv.Columns[i].Width = 55;
            }
            string title_first = (string)dgvResults.Columns[13].HeaderCell.Value;
            string title_second = (string)dgvResults.Columns[14].HeaderCell.Value;
            dgv.Columns[0].HeaderCell.Value = title_first;
            dgv.Columns[1].HeaderCell.Value = title_second;
            dgv.RowHeadersVisible = false;

            // for Chart
            chart2.Series.Dispose();
            chart2.Series.Clear();
            chart2.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart2.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart2.Series[title_first].Points.AddXY(i, omega_1_i[i]);
                chart2.Series[title_second].Points.AddXY(i, Epsilon_1_i[i]);
            }
        }

        private void чертёжToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawingForm df = new DrawingForm();
            df.Show();
        }
    }
}