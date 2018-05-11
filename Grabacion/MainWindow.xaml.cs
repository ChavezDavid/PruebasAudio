﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();
            waveOut = new WaveOutEvent();
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

            double acumulador = 0;
            double numMuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroMuestrasComplejas = 0;
            int bitsMaximos = 0;

            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximos < numMuestras);

            //bitsMaximos = 

            exponente -= 2;
            numeroMuestrasComplejas = bitsMaximos / 2;

            Complex[] muestrasComplejas = new Complex[numeroMuestrasComplejas];

            for (int i = 0; i < bytesGrabados; i+=2)
            {
                //byte i =      0 1 1 0 0 1 1 1 0 0 0 0 0 0 0 0
                //byte i+1 =    0 0 0 0 0 0 0 0 1 0 1 1 0 1 0 1
                //or =          0 1 1 0 0 1 1 1 1 0 1 1 0 1 0 1
                short muestra = (short)Math.Abs((buffer[i + 1] << 8) | buffer[i]);

                float muestra32bits = (float)muestra / 32768.0f;
                sldVolumen.Value = Math.Abs(muestra32bits);
                if (i / 2 < numeroMuestrasComplejas)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }

                //lblMuestra.Text = muestra.ToString();
                //sldVolumen.Value = muestra;

                //acumulador += muestra;
                //numMuestras++;
            }
            //double promedio = acumulador / numMuestras;
            //sldVolumen.Value = promedio;
            //writer.Write(buffer, 0, bytesGrabados);

            FastFourierTransform.FFT(true, exponente, muestrasComplejas);
            float[] valoresAbsolutos = new float[muestrasComplejas.Length];
            
            for(int i=0; i < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float)Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].X) + (muestrasComplejas[i].Y * muestrasComplejas[i].Y));
            }

            int indiceMaximo = valoresAbsolutos.ToList().IndexOf(valoresAbsolutos.Max());
            float frecuenciaFundamental = (float)(indiceMaximo * waveIn.WaveFormat.SampleRate) / (float)valoresAbsolutos.Length;
            lblFrecuencia.Text = frecuenciaFundamental.ToString();
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
    }
}
