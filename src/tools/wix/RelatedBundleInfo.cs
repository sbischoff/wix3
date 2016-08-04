// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Xml;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Utility class for Burn RelatedBundle information.
    /// </summary>
    internal class RelatedBundleInfo
    {
        public RelatedBundleInfo(Row row)
            : this((string)row[0], (int)row[1], (int)row[2])
        {
        }

        public RelatedBundleInfo(string id, int action, int defaultUpgradeRelation)
        {
            this.Id = id;
            this.Action = (Wix.RelatedBundle.ActionType)action;
            this.DefaultUpgradeRelation = defaultUpgradeRelation;
        }

        public string Id { get; private set; }
        public Wix.RelatedBundle.ActionType Action { get; private set; }

        public int DefaultUpgradeRelation { get; private set; }

        /// <summary>
        /// Generates Burn manifest element for a RelatedBundle.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlTextWriter writer)
        {
            string actionString = this.Action.ToString();

            writer.WriteStartElement("RelatedBundle");
            writer.WriteAttributeString("Id", this.Id);
            writer.WriteAttributeString("Action", Convert.ToString(this.Action));
            writer.WriteAttributeString("DefaultUpgradeRelation", this.DefaultUpgradeRelation.ToString());
            writer.WriteEndElement();
        }
    }
}
