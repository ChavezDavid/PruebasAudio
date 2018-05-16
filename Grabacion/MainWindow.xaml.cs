using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using NAudio;
using NAudio.Wave;
using NAudio.Dsp;

namespace Grabacion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn waveIn;
        WaveFormat formato;
        WaveFileWriter writer;
        WaveOutEvent waveOut;
        AudioFileReader reader;
        int puntaje = 0;
        bool poder = false;
        int timerPoder = 10;
        int cronometro = 100;
        double margenBoton;
        double canvasLeft;

        public MainWindow()
        {
            InitializeComponent();
            waveOut = new WaveOutEvent();
            canvasLeft = Canvas.GetLeft(canvasuno);
        }

        private void btnIniGrabacion_Click(object sender, RoutedEventArgs e)
        {
            waveIn = new WaveIn();
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = waveIn.WaveFormat;

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;

            writer = new WaveFileWriter("sonido.wav", formato);

            waveIn.StartRecording();
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            writer.Dispose();
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            double numMuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroMuestrasComplejas = 0;
            int bitsMaximos = 0;

            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximos < numMuestras);

            exponente -= 2;
            numeroMuestrasComplejas = bitsMaximos / 2;

            Complex[] muestrasComplejas = new Complex[numeroMuestrasComplejas];

            for (int i = 0; i < bytesGrabados; i+=2)
            {
                short muestra = (short)Math.Abs((buffer[i + 1] << 8) | buffer[i]);

                float muestra32bits = (float)muestra / 32768.0f;
                sldVolumen.Value = Math.Abs(muestra32bits);
                if (i / 2 < numeroMuestrasComplejas)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }
            }

            FastFourierTransform.FFT(true, exponente, muestrasComplejas);
            float[] valoresAbsolutos = new float[muestrasComplejas.Length];
            
            for(int i=0; i < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float)Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].X) + (muestrasComplejas[i].Y * muestrasComplejas[i].Y));
            }

            int indiceMaximo = valoresAbsolutos.ToList().IndexOf(valoresAbsolutos.Max());
            float frecuenciaFundamental = (float)(indiceMaximo * waveIn.WaveFormat.SampleRate) / (float)valoresAbsolutos.Length;
            lblFrecuencia.Text = frecuenciaFundamental.ToString();

            detectarFrecuencia(frecuenciaFundamental);
        }

        private void btnDetGrabacion_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }

        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        {
            reader = new AudioFileReader("sonido.wav");
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                waveOut.Init(reader);
                waveOut.Play();
            }
        }

        void detectarFrecuencia(float frecuenciaFundamental)
        {
            int boton = 0;

            if(frecuenciaFundamental > 420 && frecuenciaFundamental < 460)
            {
                boton = 1;
            }
            if (frecuenciaFundamental > 473 && frecuenciaFundamental < 503)
            {
                boton = 2;
            }
            if (frecuenciaFundamental > 513 && frecuenciaFundamental < 543)
            {
                boton = 3;
            }
            if (frecuenciaFundamental > 567 && frecuenciaFundamental < 607)
            {
                boton = 4;
            }
            if (frecuenciaFundamental > 639 && frecuenciaFundamental < 669)
            {
                boton = 5;
            }
            if (frecuenciaFundamental > 688 && frecuenciaFundamental < 718)
            {
                boton = 6;
            }
            if (frecuenciaFundamental > 763 && frecuenciaFundamental < 803)
            {
                boton = 7;
            }

            pulsarBoton(boton);
        }

        void pulsarBoton(int boton)
        {
            bool acierto=false;
            margenBoton = Canvas.GetLeft(greenButton);
            txtMargen.Text = Convert.ToString(margenBoton);
            txtMargenImage.Text = Convert.ToString(canvasLeft);

            if (boton == 1 && margenBoton <= canvasLeft && margenBoton >= canvasLeft)
            {
                acierto = true;
            }
            if (boton == 2)
            {
                acierto = true;
            }
            if (boton == 3)
            {
                acierto = true;
            }
            if (boton == 4)
            {
                acierto = true;
            }
            if (boton == 5)
            {
                acierto = true;
            }
            if (boton == 6)
            {
                acierto = true;
            }
            if (boton == 7)
            {
                poder = true;
            }

            if (acierto)
            {
                puntaje += 100;
            }

            if (poder)
            {
                activarPoder();
            }

            acierto = false;
            lblPuntaje.Text = Convert.ToString(puntaje);
        }

        void activarPoder()
        {
            txtPoder.Text = "Activado";
            if (cronometro <= 0)
            {
                txtPoder.Text = "Desactivado";
                cronometro = 100;
                poder = false;
            }
            cronometro -= 1;
        }
    }
}
