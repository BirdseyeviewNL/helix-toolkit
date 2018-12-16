﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FileLoadDemo
{

    using DemoCore;
    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using Microsoft.Win32;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public class MainViewModel : BaseViewModel
    {
        private string OpenFileFilter = $"3D model files ({HelixToolkit.Wpf.SharpDX.Assimp.Importer.SupportedFormatsString + " | " + HelixToolkit.Wpf.SharpDX.Assimp.Importer.SupportedFormatsString})";

        private bool showWireframe = false;
        public bool ShowWireframe
        {
            set
            {
                if (SetValue(ref showWireframe, value))
                {
                    ShowWireframeFunct(value);
                }
            }
            get
            {
                return showWireframe;
            }
        }

        public ICommand OpenFileCommand
        {
            get; set;
        }

        public ICommand ResetCameraCommand
        {
            set; get;
        }

        public ICommand ExportCommand { private set; get; }

        private bool isLoading = false;
        public bool IsLoading
        {
            private set => SetValue(ref isLoading, value);
            get => isLoading;
        }

        public SceneNodeGroupModel3D GroupModel { get; } = new SceneNodeGroupModel3D();

        private SynchronizationContext context = SynchronizationContext.Current;
        public MainViewModel()
        {
            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            EffectsManager = new DefaultEffectsManager();
            Camera = new OrthographicCamera()
            {
                LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -10, -10),
                Position = new System.Windows.Media.Media3D.Point3D(0, 10, 10),
                UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                FarPlaneDistance = 1000,
                NearPlaneDistance = 0.1
            };
            ResetCameraCommand = new DelegateCommand(() =>
            {
                (Camera as OrthographicCamera).Reset();
                (Camera as OrthographicCamera).FarPlaneDistance = 1000;
            });
            ExportCommand = new DelegateCommand(() => { ExportFile(); });
        }

        private void OpenFile()
        {
            if (isLoading)
            {
                return;
            }
            string path = OpenFileDialog(OpenFileFilter);
            if (path == null)
            {
                return;
            }
            GroupModel.Clear();
            IsLoading = true;
            Task.Run(() =>
            {
                var loader = new HelixToolkit.Wpf.SharpDX.Assimp.Importer();
                return loader.Load(path);
            }).ContinueWith((result) =>
            {
                IsLoading = false;
                if (result.IsCompleted)
                {
                    GroupModel.AddNode(result.Result.Root);
                }
                else if (result.IsFaulted && result.Exception != null)
                {
                    MessageBox.Show(result.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ExportFile()
        {
            string path = SaveFileDialog("3D model files (*.obj;|*.obj;");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
        }


        private string OpenFileDialog(string filter)
        {
            var d = new OpenFileDialog();
            d.CustomPlaces.Clear();

            d.Filter = filter;

            if (!d.ShowDialog().Value)
            {
                return null;
            }

            return d.FileName;
        }

        private string SaveFileDialog(string filter)
        {
            var d = new SaveFileDialog();
            d.Filter = filter;
            if (d.ShowDialog() == true)
            {
                return d.FileName;
            }
            else { return ""; }
        }

        private void ShowWireframeFunct(bool show)
        {
            foreach(var node in GroupModel.GroupNode.Items.PreorderDFT((node) =>
            {
                return node.IsRenderable;
            }))
            {
                if (node is MeshNode m)
                {
                    m.RenderWireframe = show;
                }
            }
        }
    }

}