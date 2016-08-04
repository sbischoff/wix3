// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Utility class for Burn MsiProperty information.
    /// </summary>
    internal class MsiPropertyInfo
    {
        public MsiPropertyInfo(Row row)
            : this((string)row[0], (string)row[1], (string)row[2])
        {
        }

        public MsiPropertyInfo(string packageId, string name, string value)
        {
            this.PackageId = packageId;
            this.Name = name;
            this.Value = value;
        }

        public string PackageId { get; private set; }
        public string Name { get; private set; }
        public string Value { get; set; }
    }

    internal class MsiTransformInfo
    {
        public MsiTransformInfo(Row row)
            : this((string)row[0], (string)row[1], (string)row[2])
        {
        }

        public MsiTransformInfo(string id, string bundleTransformId, string packageId)
        {
            this.PackageId = packageId;
            this.Id = id;
            this.BundleTransformId = bundleTransformId;
        }

        public string PackageId { get; private set; }

        public string BundleTransformId { get; private set; }

        public string Id { get; set; }

        public string ProductCode { get; private set; }

        public string UpgradeCode { get; private set; }

        public void ExtractInstanceTransformationFromMsi(string filePath)
        {
            // Aus dem bestehenden MSI Paket alle Storages abfragen
            Microsoft.Deployment.WindowsInstaller.Database msiDb = new Microsoft.Deployment.WindowsInstaller.Database(filePath, Microsoft.Deployment.WindowsInstaller.DatabaseOpenMode.ReadOnly);
            Microsoft.Deployment.WindowsInstaller.View view = msiDb.OpenView("SELECT `Name`, `Data` FROM `_Storages`");
            view.Execute();

            var record = view.Fetch();
            while (record != null)
            {
                // Der Name des Storages entspricht dem Namen der Transformation (ID)
                string storageName = record.GetString("Name");
                if (storageName == Id)
                {
                    // Variablen f¸r die einzelnen Daten einer Instanztransformation
                    string productCode = null;
                    string upgradeCode = null;

                    // Um die Informationen extrahieren zu kˆnnen, muss der Storage in einer Datei zum lesen zwischengespeichert werden.
                    string tempFilePath = Path.Combine(Path.GetTempPath(), $"{storageName}.mst");
                    string tempUnbindPath = Path.Combine(Path.GetTempPath(), $"{storageName}_UNBIND");
                    record.GetStream("Data", tempFilePath);

                    // ‹ber den Unbinder von WIX kann nun die Tabellenstruktur der Transformation geladen werden
                    Unbinder unbinder = new Unbinder();
                    unbinder.SuppressDemodularization = true;
                    unbinder.SuppressExtractCabinets = true;

                    Output output = unbinder.Unbind(tempFilePath, OutputType.Transform, tempUnbindPath);
                    foreach (Row propertyRow in output.Tables["Property"].Rows)
                    {
                        var propertyField = propertyRow.Fields.Where(u => u.Column.Name == "Property").FirstOrDefault();
                        var valueField = propertyRow.Fields.Where(u => u.Column.Name == "Value").FirstOrDefault();

                        switch (propertyField.Data.ToString())
                        {
                            case "ProductCode":
                                productCode = valueField.Data.ToString();
                                break;
                            case "UpgradeCode":
                                upgradeCode = valueField.Data.ToString();
                                break;
                        }
                    }


                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);

                    if (Directory.Exists(tempUnbindPath))
                        Directory.Delete(tempUnbindPath, true);


                    // Wenn alle Notwendigen Daten gesetzt wurden, handelt es sich mit groﬂer wahrscheinlichkeit um eine Instanztransformation
                    // und dies kann angenommen werden.
                    if (!String.IsNullOrEmpty(productCode) && !String.IsNullOrEmpty(upgradeCode))
                    {
                        ProductCode = productCode;
                        UpgradeCode = upgradeCode;
                    }
                }

                record = view.Fetch();
            }
        }
    }
}
