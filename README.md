![MakeIcon icon](Resources/MakeIcon-48.png)

This is a simple application to build Windows 10 compatible ICO icon files from multiple PNG image files.

## Usage

This utility app has no main screen and uses only a pair of open and save dialogs. When launching the app, you will be immediately presented with a dialog to select which images you want to use to compose the icon. If there are no issues with the chosen files, another dialog will be presented asking for the name and location where to save the built icon. This process will repeat indefinitely until the cancel button is pressed on either dialog.

Note that the app will perform very little checks on the images to be used to compose the icon, notably that it is sqared (same values for width and height), uses 32-bit colours (8- and 24-bit images, although valid for PNG files, will not be allowed) and no two images have the same dimensions. Otherwise, any image will be accepted.

There are no requirements on how many images or which dimensions the icon should have but it is recommended that you provide at least five images for a good Windows 10 icon: 16x16, 32x32, 48x48, 64x64 and 256x256. Note that Windows will generate internally an icon for 96x96 based on the 256x256 icon. If you need/want better control over this image, you may include a version of your image with this dimension.