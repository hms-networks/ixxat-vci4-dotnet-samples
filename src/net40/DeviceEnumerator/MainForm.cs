using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ixxat.Vci4;

namespace DeviceEnumerator
{
  public class MainForm : Form
  {
    private StatusStrip statusStrip;
    private ListView deviceList;
    private ColumnHeader VCIID;
    private ColumnHeader DeviceClass;
    private ColumnHeader DriverVersion;
    private ColumnHeader HardwareVersion;
    private ColumnHeader HardwareId;
    private ColumnHeader Description;
    private ColumnHeader Manufacturer;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// VCI specific variables.
    /// </summary>
    private Ixxat.Vci4.IVciDeviceManager DeviceManager;
    private Ixxat.Vci4.IVciDeviceList DeviceEnum;
    private System.Threading.AutoResetEvent EnumChgEvent;
    private ToolStripStatusLabel labelVciVersion;
    private ToolStripStatusLabel VciVersion;
    private System.Threading.RegisteredWaitHandle EnumChgHandle;

    public MainForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // initialize the VCI device manager
      //
      DeviceManager = VciServer.Instance().DeviceManager;

      //
      // create a VCI device enumerator
      //
      DeviceEnum = DeviceManager.GetDeviceList();

      //
      // assign an AutoResetEvent to the enumerator
      //
      EnumChgEvent = new AutoResetEvent(false);
      DeviceEnum.AssignEvent(EnumChgEvent);

      //
      // register change handler for driver collection
      //
      EnumChgHandle = ThreadPool.RegisterWaitForSingleObject(
        EnumChgEvent, new WaitOrTimerCallback(OnAsyncEnumChange),
        this, -1, false);
    }

    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.labelVciVersion = new System.Windows.Forms.ToolStripStatusLabel();
      this.VciVersion = new System.Windows.Forms.ToolStripStatusLabel();
      this.deviceList = new System.Windows.Forms.ListView();
      this.VCIID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.DriverVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.HardwareVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.DeviceClass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.HardwareId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Description = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Manufacturer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.statusStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip
      // 
      this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelVciVersion,
            this.VciVersion});
      this.statusStrip.Location = new System.Drawing.Point(0, 184);
      this.statusStrip.Name = "statusStrip";
      this.statusStrip.Size = new System.Drawing.Size(897, 22);
      this.statusStrip.TabIndex = 0;
      this.statusStrip.Text = "statusStrip1";
      // 
      // labelVciVersion
      // 
      this.labelVciVersion.Name = "labelVciVersion";
      this.labelVciVersion.Size = new System.Drawing.Size(63, 17);
      this.labelVciVersion.Text = "VciVersion:";
      // 
      // VciVersion
      // 
      this.VciVersion.Name = "VciVersion";
      this.VciVersion.Size = new System.Drawing.Size(0, 17);
      // 
      // deviceList
      // 
      this.deviceList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.VCIID,
            this.DriverVersion,
            this.HardwareVersion,
            this.DeviceClass,
            this.HardwareId,
            this.Description,
            this.Manufacturer});
      this.deviceList.Dock = System.Windows.Forms.DockStyle.Fill;
      this.deviceList.FullRowSelect = true;
      this.deviceList.GridLines = true;
      this.deviceList.HideSelection = false;
      this.deviceList.Location = new System.Drawing.Point(0, 0);
      this.deviceList.MultiSelect = false;
      this.deviceList.Name = "deviceList";
      this.deviceList.Size = new System.Drawing.Size(897, 184);
      this.deviceList.TabIndex = 1;
      this.deviceList.UseCompatibleStateImageBehavior = false;
      this.deviceList.View = System.Windows.Forms.View.Details;
      // 
      // VCIID
      // 
      this.VCIID.Text = "VCIID";
      this.VCIID.Width = 40;
      // 
      // DriverVersion
      // 
      this.DriverVersion.Text = "Driver Version";
      this.DriverVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.DriverVersion.Width = 80;
      // 
      // HardwareVersion
      // 
      this.HardwareVersion.Text = "Hardware Version";
      this.HardwareVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.HardwareVersion.Width = 80;
      // 
      // DeviceClass
      // 
      this.DeviceClass.Text = "Device Class";
      this.DeviceClass.Width = 220;
      // 
      // HardwareId
      // 
      this.HardwareId.Text = "Hardware ID";
      this.HardwareId.Width = 80;
      // 
      // Description
      // 
      this.Description.Text = "Description";
      this.Description.Width = 180;
      // 
      // Manufacturer
      // 
      this.Manufacturer.Text = "Manufacturer";
      this.Manufacturer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.Manufacturer.Width = 200;
      // 
      // MainForm
      // 
      this.ClientSize = new System.Drawing.Size(897, 206);
      this.Controls.Add(this.deviceList);
      this.Controls.Add(this.statusStrip);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "MainForm";
      this.Load += new System.EventHandler(this.OnMainFormLoad);
      this.statusStrip.ResumeLayout(false);
      this.statusStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }

        IDisposable dispose;

        // Dispose device list
        dispose = DeviceEnum as IDisposable;
        if (null != dispose)
        {
          dispose.Dispose();
        }

        // Dispose device manager
        dispose = DeviceManager as IDisposable;
        if (null != dispose)
        {
          dispose.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    ///-----------------------------------------------------------------------
    /// <summary>
    /// Called when the main form is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMainFormLoad(object sender, System.EventArgs e)
    {
      //
      // retrieve version info
      //
      VciVersion.Text = VciServer.Instance().Version.ToString();
      //
      // simulate a change of the device list to update the list
      // list (see DataChangeEvent)
      //
      EnumChgEvent.Set();
    }

    //------------------------------------------------------------------------
    /// <summary>
    /// Called after the AutoResetEvent assigned to the device enumerator
    /// has been signaled.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="timedOut"></param>
    private void OnAsyncEnumChange(object state, bool timedOut)
    {
      if (!timedOut)
        this.Invoke(new EventHandler(DataChangeEvent));
    }

    //------------------------------------------------------------------------    
    /// <summary>
    /// Called after the contents of the device list has changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataChangeEvent(object sender, System.EventArgs e)
    {
      this.deviceList.BeginUpdate();
      this.deviceList.Items.Clear();

      foreach (IVciDevice i in DeviceEnum)
      {
        ListViewItem item = new ListViewItem(i.VciObjectId.ToString());

        item.SubItems.Add(i.DriverVersion.ToString());
        item.SubItems.Add(i.HardwareVersion.ToString());

        item.SubItems.Add(i.DeviceClass.ToString());
        item.SubItems.Add(i.UniqueHardwareId.ToString());
        item.SubItems.Add(i.Description);
        item.SubItems.Add(i.Manufacturer);

        this.deviceList.Items.Add(item);

        (i as IDisposable).Dispose();
      }

      this.deviceList.EndUpdate();
    }
  }
}
