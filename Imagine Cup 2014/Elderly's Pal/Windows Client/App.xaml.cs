//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using NiceDreamers.Windows.ViewModels;

namespace NiceDreamers.Windows
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
    {
        /// <summary>
        ///     Catalog of exported parts from an assembly
        /// </summary>
        private AssemblyCatalog catalog;

        /// <summary>
        ///     Managed Entity Framework composition container used to compose the entity graph
        /// </summary>
        private CompositionContainer compositionContainer;

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    catalog.Dispose();
                    compositionContainer.Dispose();
                }
            }

            disposed = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Catalog all exported parts within this assembly
            catalog = new AssemblyCatalog(typeof (App).Assembly);
            compositionContainer = new CompositionContainer(catalog);

            Window window = new MainWindow(compositionContainer.GetExportedValue<KinectController>());
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Dispose();
        }
    }
}