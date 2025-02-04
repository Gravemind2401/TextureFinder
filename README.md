This is just a simple texture finder utility to find texture data inside binary files.

It supports a number of DXGI formats, Xbox-specific formats and jpeg/png etc image data. It also supports finding textures within Deflate and ZLib streams.

## Usage ##
- The `Address` field controls the address within the file where it will start reading texture data.
- The `Offset` field controls how many bytes to skip ahead of `Address` before reading image data. This is useful for compressed data where `Address` will be the start of the compressed data and `Offset` will be the offset of the image data within that compressed section.
- The `Step` field controls how much the address and offset will increment or decrement by when clicking the + and - buttons.

### Notes ###
In the `Format Mode` dropdown, the `ImageFormat` mode will try to detect embedded jpeg/png files as well as some other common formats.

Note that the `ImageFormat` mode will ignore whatever dimensions are specified - it will just use the dimensions in the image file

If there is no image showing at all (the view is solid blue) then either you are looking at a solid blue image or the image failed to load.
The image may fail to load because;
- You are using a decompression mode but there is no compressed data starting at `Address`
- There are not enough bytes available to load an image with the given dimensions (try using smaller dimensions or an address thats not right near the end of the file)
