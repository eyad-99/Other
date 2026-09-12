import { Component } from '@angular/core';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  progress = 0;

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;

    this.http.get<{ uploadUrl: string }>(
      `https://localhost:7191/api/blob/sas?blobName=${file.name}`
    ).subscribe(response => {
      const sasUrl = response.uploadUrl;

      const headers = new HttpHeaders({ 'x-ms-blob-type': 'BlockBlob' });
      this.http.put(sasUrl, file, {
        headers: headers,
        reportProgress: true,
        observe: 'events'
      }).subscribe(event => {
        if (event.type === HttpEventType.UploadProgress) {
          if (event.total) {
            this.progress = Math.round(100 * event.loaded / event.total);
          } else {
            this.progress = Math.round((event.loaded / file.size) * 100);
          }
        } else if (event.type === HttpEventType.Response) {
          console.log('Upload complete');
        }
      });
    });
  }
}
