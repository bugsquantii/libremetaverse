using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Example of how to put a new script in your inventory
    /// </summary>
    public class UploadScriptCommand : Command
    {
        /// <summary>
        ///  The default constructor for TestClient commands
        /// </summary>
        /// <param name="testClient"></param>
        public UploadScriptCommand(TestClient testClient)
        {
            Name = "uploadscript";
            Description = "Upload a local .lsl file file into your inventory.";
            Category = CommandCategory.Inventory;
        }

        /// <summary>
        /// The default override for TestClient commands
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fromAgentId"></param>
        /// <returns></returns>
        public override string Execute(string[] args, UUID fromAgentId)
        {
            if (args.Length < 1)
                return "Usage: uploadscript filename.lsl";

            var file = args.Aggregate(string.Empty, (current, t) => $"{current}{t} ");
            file = file.TrimEnd();

            if (!File.Exists(file))
                return $"Filename '{file}' does not exist";

            var ret = $"Filename: {file}";

            try
            {
                using (var reader = new StreamReader(file))
                {
                    var body = reader.ReadToEnd();
                    var desc = $"{file} created by OpenMetaverse TestClient {DateTime.Now}";
                    // create the asset
                    Client.Inventory.RequestCreateItem(Client.Inventory.FindFolderForType(AssetType.LSLText),
                        file, desc, AssetType.LSLText, UUID.Random(),
                        InventoryType.LSL, PermissionMask.All, (success, item) =>
                        {
                            if (success)
                                // upload the asset
                                Client.Inventory.RequestUpdateScriptAgentInventory(
                                    System.Text.Encoding.UTF8.GetBytes(body), item.UUID, true,
                                    (uploadSuccess, uploadStatus, compileSuccess, compileMessages, itemId, assetId) =>
                                    {
                                        if (uploadSuccess)
                                            ret += $" Script successfully uploaded, ItemID {itemId} AssetID {assetId}";
                                        if (compileSuccess)
                                            ret += " compilation successful";
                                    });
                        });
                }
                return ret;

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), Helpers.LogLevel.Error, Client);
                return $"Error creating script for {ret}";
            }
        }
    }

}
