﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.DataFormats;

namespace ParkingReservationApp
{
    public partial class Reservation : Form
    {
        public const string StoreData = "Parking.json";
        // parking slot
        private string[] parkingSlots;
        // waiting list
        private Queue<string> waitingQueue = new Queue<string>();
        // history
        private Stack<string> parkingHistory = new Stack<string>();
        // for unique plate numbers
        private HashSet<string> parkedCars = new HashSet<string>();
        // specifying parking lot size
        private int parkingLotSize = 10;
        public class ParkingData
        {
            public string[] ParkingSlots { get; set; }
            public List<string> WaitingQueue { get; set; }
            public List<string> ParkingHistory { get; set; }
            public List<string> ParkedCars { get; set; }
        }
        public Reservation()
        {
            InitializeComponent();
        }
        private void Reservation_Load(object sender, EventArgs e)
        {
            parkingSlots = new string[parkingLotSize];
            LoadData();
            UpdateUI();
        }
        private void ParkCar(string plateNumber)
        {
            string timeFormat = "HH:mm";
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                MessageBox.Show("Please enter a valid license plate.");
                return;
            }
            if (parkedCars.Contains(plateNumber))
            {
                MessageBox.Show("This car is already parked.");
                return;
            }
            // searches for an empty slot
            for (int i = 0; i < parkingSlots.Length; i++)
            {
                if (parkingSlots[i] == null)
                {
                    parkingSlots[i] = plateNumber;
                    parkedCars.Add(plateNumber);
                    parkingHistory.Push($"Parked|{plateNumber}|{DateTime.Now.ToString(timeFormat)}");
                    UpdateUI();
                    return;

                }
            }
            // add to waiting list
            waitingQueue.Enqueue(plateNumber);
            MessageBox.Show("Parking lot is full. Added to the waiting list.");
            Save_Data();
            UpdateUI();
        }
        private void UnparkCar(string plateNumber)
        {
            string timeFormat = "HH:mm";
            if (!parkedCars.Contains(plateNumber))
            {
                MessageBox.Show("Car not found in the parking lot.");
                return;
            }
            // unpark
            for (int i = 0; i < parkingSlots.Length; i++)
            {
                if (parkingSlots[i] == plateNumber)
                {
                    parkingSlots[i] = null;
                    parkedCars.Remove(plateNumber);
                    parkingHistory.Push($"Unparked|{plateNumber}|{DateTime.Now.ToString(timeFormat)}");
                    MessageBox.Show($"Car {plateNumber} has been removed.");
                    break;
                }
            }
            // moves car from waiting list to the parking lot
            if (waitingQueue.Count > 0)
            {
                string nextCar = waitingQueue.Dequeue();
                for (int i = 0; i < parkingSlots.Length; i++)
                {
                    if (parkingSlots[i] == null)
                    {
                        parkingSlots[i] = nextCar;
                        parkedCars.Add(nextCar);
                        parkingHistory.Push($"Parked|{nextCar}|{DateTime.Now.ToString(timeFormat)}");
                        MessageBox.Show($"Car {nextCar} has been parked from the waiting list.");

                        break;
                    }
                }
            }
            Save_Data();
            UpdateUI();

        }
        private void UpdateWaitingList()
        {
            waitingGridView.Rows.Clear();

            int position = 1;
            foreach (string plateNumber in waitingQueue)
            {
                waitingGridView.Rows.Add(position++, plateNumber);
            }
        }
        private void UpdateHistory()
        {
            historyGridView.Rows.Clear();
            foreach (string action in parkingHistory)
            {
                string[] details = action.Split('|');
                historyGridView.Rows.Add(details[0], details[1], details[2]);
            }
        }
        private void UpdateParkingStatus()
        {
            parkingGridView.Rows.Clear();
            for (int i = 0; i < parkingSlots.Length; i++)
            {
                string slotStatus = parkingSlots[i] == null ? "Available" : "Occupied";
                string plateNumber = parkingSlots[i] ?? "Empty";
                // adds row to the data grid view
                parkingGridView.Rows.Add($"Slot {i + 1}", plateNumber, slotStatus);
            }
        }
        private void UpdateUI()
        {
            UpdateParkingStatus();
            UpdateWaitingList();
            UpdateHistory();
        }


        private void pictureBox1_Click(object sender, EventArgs e)//ParkUnparkbtn
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = true;
        }
        private void Save_Data()
        {
            var data = new ParkingData
            {
                ParkingSlots = parkingSlots,
                WaitingQueue = waitingQueue.ToList(),
                ParkingHistory = parkingHistory.ToList(),
                ParkedCars = parkedCars.ToList(),
            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(StoreData, json);
        }
        private void LoadData()
        {
            if (File.Exists(StoreData))
            {
                var json = File.ReadAllText(StoreData);
                var data = JsonConvert.DeserializeObject<ParkingData>(json);


                parkingSlots = data.ParkingSlots ?? new string[parkingLotSize];
                waitingQueue = new Queue<string>(data.WaitingQueue ?? new List<string>());
                parkingHistory = new Stack<string>((data.ParkingHistory ?? new List<string>()).AsEnumerable().Reverse());
                parkedCars = new HashSet<string>(data.ParkedCars ?? new List<string>());

                UpdateUI();
            }
            else
            {
                parkingSlots = new string[parkingLotSize];
            }

        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            string plateNumber = txtlicense.Text;
            ParkCar(plateNumber);
            txtlicense.Clear();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            string plateNumber = txtlicense.Text;
            UnparkCar(plateNumber);
            txtlicense.Clear();
        }

        private void waitingListbtn_Click(object sender, EventArgs e)
        {

            panel1.Visible = true;
            panel2.Visible = false;
            panel3.Visible = false;
        }

        private void Historybtn_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel2.Visible = true;
            panel3.Visible = false;
        }

        private void Exitbtn_Click(object sender, EventArgs e)
        {
            
        }

        
        private void Reservationbtn_Click(object sender, EventArgs e)
        {
            Form3 Reserve = new Form3();
            this.Hide(); Reserve.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            string plateNumber = txtlicense.Text;
            ParkCar(plateNumber);
            txtlicense.Clear();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            string plateNumber = txtlicense.Text;
            UnparkCar(plateNumber);
            txtlicense.Clear();
        }
    }
}
