import { Component, signal } from '@angular/core';
import {
  HttpClient,
  HttpEventType,
  HttpHeaders
} from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {

  progress = signal(0);
  uploading = signal(false);
  uploadComplete = signal(false);
  errorMessage = signal('');

  constructor(private http: HttpClient) {}

  onFileSelected(event: Event): void {

    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    // Reset
    this.progress.set(0);
    this.uploading.set(true);
    this.uploadComplete.set(false);
    this.errorMessage.set('');

    console.log('Selected file:', file.name);
    console.log('File size:', file.size);

    const blobName = encodeURIComponent(file.name);

    // ----------------------------------------
    // STEP 1: Get SAS URL
    // ----------------------------------------

    this.http.get<{ uploadUrl: string }>(
      `https://localhost:7191/api/blob/sas?blobName=${blobName}`
    ).subscribe({

      next: response => {

        console.log('SAS URL received');

        const sasUrl = response.uploadUrl;

        const headers = new HttpHeaders({
          'x-ms-blob-type': 'BlockBlob',
          'Content-Type': file.type || 'application/octet-stream'
        });

        // ----------------------------------------
        // STEP 2: Upload file to Azure
        // ----------------------------------------

        this.http.put(sasUrl, file, {
          headers: headers,
          reportProgress: true,
          observe: 'events'
        }).subscribe({

          next: event => {

            // ----------------------------------------
            // UPLOAD PROGRESS
            // ----------------------------------------

            if (event.type === HttpEventType.UploadProgress) {

              const loaded = event.loaded;

              const total = event.total ?? file.size;

              const percentage = Math.round(
                (loaded / total) * 100
              );

              console.log(
                `Uploaded: ${loaded} / ${total}`
              );

              console.log(
                `Progress: ${percentage}%`
              );

              // Update signal
              this.progress.set(percentage);
            }

            // ----------------------------------------
            // UPLOAD COMPLETE
            // ----------------------------------------

            if (event.type === HttpEventType.Response) {

              console.log('Upload complete');

              this.progress.set(100);
              this.uploading.set(false);
              this.uploadComplete.set(true);
            }
          },

          error: error => {

            console.error(
              'Upload error:',
              error
            );

            this.uploading.set(false);
            this.uploadComplete.set(false);

            this.errorMessage.set(
              'Upload failed.'
            );
          }
        });
      },

      error: error => {

        console.error(
          'SAS URL error:',
          error
        );

        this.uploading.set(false);

        this.errorMessage.set(
          'Could not get SAS URL.'
        );
      }
    });
  }
}
