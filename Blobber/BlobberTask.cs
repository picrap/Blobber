﻿#region Blobber!
// Blobber - Merges or embed referenced assemblies
// https://github.com/picrap/Blobber
// MIT License - http://opensource.org/licenses/MIT
#endregion

using Blobber;

public class BlobberTask : StitcherTask<BlobberStitcher>
{
    public static int Main(string[] args)
    {
        try
        {
            return Run(new BlobberTask(), args);
        }
        catch { }
        return -1;
    }
}