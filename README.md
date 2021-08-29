# Unity NFT Storage Client

This repository contains a self-contained [nft.storage](https://nft.storage/) client library fully compatible with [Unity Engine](https://www.unity.com/).

## Setup

For using this library, just copy the [NFTStorageClient.cs](./NFTStorageClient.cs) source code file to your Unity project, preferably alongside other scripts. As any regular Unity C# script, the main class defined in this library inherits Unity's [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) class.

## Usage

Anyone registered to *nft.storage* (an API key is required) can use this library to store and persist game assets using IPFS system. Please check nft.storage [docs](https://nft.storage/#docs) for additional details.

The client library code is available in `NFTStorageClient.cs`, and contains the following public methods:

- **SetApiToken:** Configure nft.storage API token
- **ListFiles:** List all NFT uploaded files
- **GetFile:** Retrieve details on a specific uploaded file
- **GetFileData:** Retrieve data from a specific uploaded file
- **CheckFile:** Perform a integrity check on a specific uploaded file
- **DeleteFile:** Delete a specific uploaded file
- **UploadDataFromString:** Uploads a new file based on a data string
- **UploadDataFromFile:** Uploads a new file based on the path from the local file system

## Example Usage

This repository also contains an example game developed in Unity, and using the nft.storage client library. All the source code for the game can be found in the folder [FungibleZombies](./FungibleZombies/).

## License

This code is distributed using MIT license.
