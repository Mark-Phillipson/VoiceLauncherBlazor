# Face Recognition & Tagging Feature

## Overview

The Face Recognition & Tagging feature allows users to:
- Upload images containing faces
- Tag faces in images with names
- Search for people by name to highlight their faces
- Hover over faces to see assigned names
- Store images and face tag data in the database

## Database Setup

Before using the feature, you need to run the database migration script:

```sql
-- Run this script in your SQL Server database (VoiceLauncher)
-- Location: DataAccessLibrary/Scripts/FaceRecognition-Migration-Script.sql
```

To execute the script:
1. Open SQL Server Management Studio (SSMS)
2. Connect to your VoiceLauncher database
3. Open the file `DataAccessLibrary/Scripts/FaceRecognition-Migration-Script.sql`
4. Execute the script (F5)

This will create two tables:
- `FaceImages` - Stores uploaded images with metadata
- `FaceTags` - Stores face location and name information

## Using the Feature

### Accessing the Page

1. Run the VoiceAdmin application
2. Navigate to http://localhost:5008/face-recognition
3. Or click "Face Recognition" in the navigation menu

### Uploading an Image

1. Enter an image name in the "Image Name" field
2. (Optional) Enter a description
3. Click "Select Image" and choose an image file (max 5MB)
4. Click "Upload Image"
5. The image will appear in the "Saved Images" list and be displayed on the right

### Tagging Faces

1. Select an image from the "Saved Images" list
2. Click the "Add Face Tag" button
3. Click on a face in the image to create a tag box
4. Enter the person's first name
5. Click "Save" to store the tag
6. The face tag will appear as a blue box on the image

### Searching for People

1. Type a name in the "Search by Name" field
2. Matching face tags will be highlighted in yellow with a pulsing animation
3. Clear the search to remove highlighting

### Hovering Over Tags

- Hover your mouse over any face tag to see the person's name in a tooltip

### Deleting Tags

- Hover over a face tag
- Click the small "X" button that appears in the top-right corner
- Confirm the deletion

### Deleting Images

1. Select an image
2. Click the "Delete" button in the image viewer
3. Confirm the deletion
4. The image and all associated tags will be removed

## Architecture

### Database Models

- **FaceImage** (`DataAccessLibrary/Models/FaceImage.cs`)
  - Stores image metadata and base64-encoded image data
  - Has a one-to-many relationship with FaceTags

- **FaceTag** (`DataAccessLibrary/Models/FaceTag.cs`)
  - Stores face location (X, Y, Width, Height as percentages)
  - Stores the person's first name
  - Linked to a FaceImage via foreign key

### Data Transfer Objects (DTOs)

- `FaceImageDTO` - For transferring image data between layers
- `FaceTagDTO` - For transferring face tag data between layers

### Repositories

- `IFaceImageRepository` / `FaceImageRepository` - CRUD operations for images
- `IFaceTagRepository` / `FaceTagRepository` - CRUD operations for face tags

### Blazor Component

- `FaceRecognition.razor` - UI markup
- `FaceRecognition.razor.cs` - Component logic
- `FaceRecognition.razor.css` - Component-specific styles

## Testing

### Running Playwright Tests

The feature includes comprehensive Playwright tests located in:
`PlaywrightTests/FaceRecognitionTests.cs`

To run the tests:

1. Ensure the VoiceAdmin application is running on http://localhost:5008
2. Ensure the database migration has been executed
3. Run the tests:

```bash
cd PlaywrightTests
dotnet test --filter FaceRecognition
```

### Test Coverage

The tests include:
- Page load verification
- Image upload workflow
- Face tagging functionality
- Search and highlight feature
- Screenshots at each step for visual verification

### Screenshots

The tests generate screenshots in the test output directory:
- `face-recognition-initial.png` - Initial page state
- `face-recognition-after-upload.png` - After uploading an image
- `face-recognition-tagging-mode.png` - In tagging mode
- `face-recognition-with-tag.png` - After adding a tag
- `face-recognition-search-highlight.png` - Search highlighting
- `face-recognition-final.png` - Final state

## Technical Details

### Image Storage

Images are stored as base64-encoded strings in the database. This approach:
- Simplifies deployment (no separate file storage needed)
- Ensures data integrity
- Suitable for moderate image sizes (recommended max 5MB)

For production use with many large images, consider:
- Azure Blob Storage
- AWS S3
- Local file system with database references

### Face Tag Coordinates

Face tags are stored as percentages (0-100) to maintain proper positioning when:
- Images are displayed at different sizes
- The viewport changes
- The layout is responsive

### Accessibility

The feature includes:
- Keyboard navigation support
- ARIA labels for screen readers
- Clear focus indicators
- Descriptive button text
- Toast notifications for success/error messages

## Future Enhancements

Potential improvements:
- Automatic face detection using ML models
- Drag-to-resize face tag boxes
- Multiple tags per click
- Export tagged images
- Face recognition to suggest names
- Image galleries and albums
- Bulk image upload
- Image filters and enhancements

## Troubleshooting

### Images not loading
- Check that the database migration has been run
- Verify the connection string in appsettings.json
- Check browser console for errors

### Upload fails
- Ensure image is under 5MB
- Check that the image is a valid format (PNG, JPG, etc.)
- Verify database write permissions

### Tags not appearing
- Refresh the page
- Check browser console for JavaScript errors
- Verify the FaceTags table exists in the database

## Support

For issues or questions:
1. Check the browser console for errors
2. Review the application logs
3. Verify database connectivity
4. Check that all migrations have been applied
