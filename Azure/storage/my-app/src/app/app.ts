import { Component, signal } from '@angular/core';
import {
  HttpClient,
  HttpEventType,
  HttpHeaders
} from '@angular/common/http';

import { lastValueFrom } from 'rxjs';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {

  // =========================================================
  // UI STATE
  // =========================================================

  progress = signal(0);

  uploading = signal(false);

  uploadComplete = signal(false);

  errorMessage = signal('');

  // =========================================================
  // UPLOAD SETTINGS
  // =========================================================

  /**
   * Chunk size = 5 MB
   */
  private readonly CHUNK_SIZE =
    5 * 1024 * 1024;

  /**
   * Maximum number of attempts
   * for a failed chunk.
   */
  private readonly MAX_RETRIES = 5;

  // =========================================================
  // CONSTRUCTOR
  // =========================================================

  constructor(
    private http: HttpClient
  ) {}

  // =========================================================
  // FILE SELECTED
  // =========================================================

  async onFileSelected(
    event: Event
  ): Promise<void> {

    const input =
      event.target as HTMLInputElement;

    const file =
      input.files?.[0];

    if (!file) {
      return;
    }

    // ---------------------------------------------------------
    // Reset UI
    // ---------------------------------------------------------

    this.progress.set(0);

    this.uploading.set(true);

    this.uploadComplete.set(false);

    this.errorMessage.set('');

    console.log('====================================');
    console.log('Starting upload');
    console.log('====================================');

    console.log(
      'File:',
      file.name
    );

    console.log(
      'Size:',
      this.formatBytes(file.size)
    );

    console.log(
      'Chunk size:',
      this.formatBytes(this.CHUNK_SIZE)
    );

    try {

      // =======================================================
      // STEP 1
      // GET SAS URL
      // =======================================================

      const blobName =
        encodeURIComponent(
          file.name
        );

      console.log(
        'Requesting SAS URL...'
      );

      const sasResponse =
        await lastValueFrom(

          this.http.get<{
            uploadUrl: string
          }>(
            `https://localhost:7191/api/blob/sas?blobName=${blobName}`
          )

        );

      const sasUrl =
        sasResponse.uploadUrl;

      console.log(
        'SAS URL received.'
      );

      // =======================================================
      // STEP 2
      // CALCULATE CHUNKS
      // =======================================================

      const totalChunks =
        Math.ceil(
          file.size /
          this.CHUNK_SIZE
        );

      console.log(
        'Total chunks:',
        totalChunks
      );

      // =======================================================
      // STEP 3
      // BLOCK IDs
      // =======================================================

      const blockIds: string[] = [];

      // Total bytes completed
      let uploadedBytes = 0;

      // =======================================================
      // STEP 4
      // UPLOAD CHUNKS
      // =======================================================

      for (
        let chunkIndex = 0;
        chunkIndex < totalChunks;
        chunkIndex++
      ) {

        // -----------------------------------------------------
        // Calculate chunk range
        // -----------------------------------------------------

        const start =
          chunkIndex *
          this.CHUNK_SIZE;

        const end =
          Math.min(
            start +
            this.CHUNK_SIZE,
            file.size
          );

        const chunk =
          file.slice(
            start,
            end
          );

        // -----------------------------------------------------
        // Create block ID
        // -----------------------------------------------------

        const blockId =
          this.createBlockId(
            chunkIndex
          );

        blockIds.push(
          blockId
        );

        console.log('');
        console.log(
          '===================================='
        );

        console.log(
          `Chunk ${chunkIndex + 1}/${totalChunks}`
        );

        console.log(
          'Size:',
          this.formatBytes(chunk.size)
        );

        console.log(
          'Block ID:',
          blockId
        );

        console.log(
          '===================================='
        );

        // -----------------------------------------------------
        // Upload chunk
        // -----------------------------------------------------

        await this.uploadChunkWithRetry(

          sasUrl,

          chunk,

          blockId,

          uploadedBytes,

          file.size,

          chunkIndex,

          totalChunks

        );

        // -----------------------------------------------------
        // Chunk completely uploaded
        // -----------------------------------------------------

        uploadedBytes +=
          chunk.size;

        const completedPercentage =
          Math.round(
            (
              uploadedBytes /
              file.size
            ) * 100
          );

        this.progress.set(
          completedPercentage
        );

        console.log(
          `Chunk ${chunkIndex + 1} complete`
        );

        console.log(
          `Overall progress: ${completedPercentage}%`
        );
      }

      // =======================================================
      // STEP 5
      // ALL BLOCKS UPLOADED
      // =======================================================

      console.log('');
      console.log(
        '===================================='
      );

      console.log(
        'All chunks uploaded.'
      );

      console.log(
        'Committing block list...'
      );

      console.log(
        '===================================='
      );

      // =======================================================
      // STEP 6
      // COMMIT BLOCK LIST
      // =======================================================

      await this.commitBlockList(

        sasUrl,

        blockIds,

        file.type ||
        'application/octet-stream'

      );

      // =======================================================
      // STEP 7
      // COMPLETE
      // =======================================================

      this.progress.set(100);

      this.uploading.set(false);

      this.uploadComplete.set(true);

      console.log('');
      console.log(
        '===================================='
      );

      console.log(
        'UPLOAD COMPLETE'
      );

      console.log(
        '===================================='
      );

    } catch (error) {

      // =======================================================
      // UPLOAD FAILED
      // =======================================================

      console.error(
        'Upload failed:',
        error
      );

      this.uploading.set(false);

      this.uploadComplete.set(false);

      this.errorMessage.set(
        'Upload failed. Please try again.'
      );
    }
  }

  // =========================================================
  // UPLOAD CHUNK WITH RETRY
  // =========================================================

  private async uploadChunkWithRetry(

    sasUrl: string,

    chunk: Blob,

    blockId: string,

    uploadedBytesBeforeChunk: number,

    totalFileSize: number,

    chunkIndex: number,

    totalChunks: number

  ): Promise<void> {

    let attempt = 0;

    while (
      attempt <
      this.MAX_RETRIES
    ) {

      attempt++;

      try {

        console.log(
          `Uploading chunk ${chunkIndex + 1}/${totalChunks}`,
          `attempt ${attempt}/${this.MAX_RETRIES}`
        );

        // -----------------------------------------------------
        // Upload the actual block
        // -----------------------------------------------------

        await this.uploadChunk(

          sasUrl,

          chunk,

          blockId,

          uploadedBytesBeforeChunk,

          totalFileSize

        );

        // -----------------------------------------------------
        // IMPORTANT:
        //
        // uploadChunk() only returns after Azure sends
        // the final HTTP Response event.
        // -----------------------------------------------------

        return;

      } catch (error) {

        console.error(
          `Chunk ${chunkIndex + 1} failed.`,
          error
        );

        // -----------------------------------------------------
        // Network completely offline
        // -----------------------------------------------------

        if (!navigator.onLine) {

          console.log(
            'Network is offline.'
          );

          console.log(
            'Waiting for network...'
          );

          await this.waitForNetwork();

          console.log(
            'Network is back.'
          );
        }

        // -----------------------------------------------------
        // Maximum retries
        // -----------------------------------------------------

        if (
          attempt >=
          this.MAX_RETRIES
        ) {

          console.error(
            `Chunk ${chunkIndex + 1} failed after ${this.MAX_RETRIES} attempts.`
          );

          throw error;
        }

        // -----------------------------------------------------
        // Exponential retry delay
        //
        // Attempt 1 -> 1 second
        // Attempt 2 -> 2 seconds
        // Attempt 3 -> 4 seconds
        // Attempt 4 -> 8 seconds
        // Attempt 5 -> 10 seconds max
        // -----------------------------------------------------

        const delay =
          Math.min(
            1000 *
            Math.pow(
              2,
              attempt - 1
            ),
            10000
          );

        console.log(
          `Retrying chunk in ${delay} ms...`
        );

        await this.sleep(
          delay
        );
      }
    }
  }

  // =========================================================
  // UPLOAD ONE BLOCK TO AZURE
  // =========================================================

  private async uploadChunk(

    sasUrl: string,

    chunk: Blob,

    blockId: string,

    uploadedBytesBeforeChunk: number,

    totalFileSize: number

  ): Promise<void> {

    // ---------------------------------------------------------
    // Azure Put Block URL
    //
    // ?comp=block
    // &blockid=<Base64 block ID>
    // ---------------------------------------------------------

    const url =
      this.addQueryParameters(

        sasUrl,

        {
          comp: 'block',

          blockid: blockId
        }

      );

    const headers =
      new HttpHeaders({

        'Content-Type':
          'application/octet-stream'

      });

    // =========================================================
    // IMPORTANT
    //
    // Use lastValueFrom(), NOT firstValueFrom().
    //
    // Angular emits:
    //
    // UploadProgress
    // UploadProgress
    // UploadProgress
    // ...
    // Response
    //
    // lastValueFrom() waits for the final Response.
    // =========================================================

    await lastValueFrom(

      this.http.put(

        url,

        chunk,

        {
          headers,

          reportProgress: true,

          observe: 'events',

          responseType: 'text'
        }

      ).pipe(

        tap(event => {

          // ===================================================
          // UPLOAD PROGRESS
          // ===================================================

          if (
            event.type ===
            HttpEventType.UploadProgress
          ) {

            const currentChunkBytes =
              event.loaded;

            const totalUploaded =
              uploadedBytesBeforeChunk +
              currentChunkBytes;

            let percentage =
              Math.round(

                (
                  totalUploaded /
                  totalFileSize
                ) * 100

              );

            // Never exceed 100
            percentage =
              Math.min(
                percentage,
                100
              );

            // Update Angular signal
            this.progress.set(
              percentage
            );

            // Chunk percentage
            const chunkPercentage =
              Math.round(

                (
                  currentChunkBytes /
                  chunk.size
                ) * 100

              );

            console.log(
              `Chunk progress: ${chunkPercentage}%`
            );

            console.log(
              `Overall progress: ${percentage}%`
            );
          }

          // ===================================================
          // AZURE RESPONSE
          // ===================================================

          if (
            event.type ===
            HttpEventType.Response
          ) {

            console.log(
              'Azure block response received.'
            );
          }

        })

      )

    );
  }

  // =========================================================
  // COMMIT BLOCK LIST
  // =========================================================

  private async commitBlockList(

    sasUrl: string,

    blockIds: string[],

    contentType: string

  ): Promise<void> {

    // ---------------------------------------------------------
    // Azure Put Block List URL
    // ---------------------------------------------------------

    const url =
      this.addQueryParameters(

        sasUrl,

        {
          comp: 'blocklist'
        }

      );

    // ---------------------------------------------------------
    // Azure expects XML
    // ---------------------------------------------------------

    const xml = `
<?xml version="1.0" encoding="utf-8"?>
<BlockList>
${blockIds
  .map(
    id =>
      `  <Latest>${id}</Latest>`
  )
  .join('\n')}
</BlockList>
`.trim();

    console.log(
      'Block list XML:'
    );

    console.log(
      xml
    );

    const headers =
      new HttpHeaders({

        'Content-Type':
          'application/xml',

        'x-ms-blob-content-type':
          contentType

      });

    // ---------------------------------------------------------
    // Commit block list
    // ---------------------------------------------------------

    await lastValueFrom(

      this.http.put(

        url,

        xml,

        {
          headers,

          responseType: 'text'
        }

      )

    );

    console.log(
      'Block list committed successfully.'
    );
  }

  // =========================================================
  // CREATE BLOCK ID
  // =========================================================

  private createBlockId(
    index: number
  ): string {

    /*
     * Azure block IDs must be Base64 encoded.
     *
     * We generate:
     *
     * 000000
     * 000001
     * 000002
     * 000003
     *
     * etc.
     */

    const value =
      index
        .toString()
        .padStart(
          6,
          '0'
        );

    return btoa(
      value
    );
  }

  // =========================================================
  // WAIT FOR NETWORK
  // =========================================================

  private waitForNetwork(): Promise<void> {

    return new Promise(
      resolve => {

        // -----------------------------------------------------
        // Already online
        // -----------------------------------------------------

        if (
          navigator.onLine
        ) {

          resolve();

          return;
        }

        // -----------------------------------------------------
        // Wait for browser "online" event
        // -----------------------------------------------------

        const handler =
          () => {

            window.removeEventListener(
              'online',
              handler
            );

            resolve();
          };

        window.addEventListener(
          'online',
          handler
        );

      }
    );
  }

  // =========================================================
  // SLEEP
  // =========================================================

  private sleep(
    milliseconds: number
  ): Promise<void> {

    return new Promise(
      resolve => {

        setTimeout(
          resolve,
          milliseconds
        );

      }
    );
  }

  // =========================================================
  // ADD QUERY PARAMETERS
  // =========================================================

  private addQueryParameters(

    url: string,

    params: Record<
      string,
      string
    >

  ): string {

    const separator =
      url.includes('?')
        ? '&'
        : '?';

    const query =
      Object.entries(params)

        .map(
          ([key, value]) =>
            `${encodeURIComponent(key)}=${encodeURIComponent(value)}`
        )

        .join('&');

    return (
      `${url}${separator}${query}`
    );
  }

  // =========================================================
  // FORMAT FILE SIZE
  // =========================================================

  private formatBytes(
    bytes: number
  ): string {

    if (
      bytes === 0
    ) {

      return '0 Bytes';
    }

    const units = [
      'Bytes',
      'KB',
      'MB',
      'GB'
    ];

    const index =
      Math.floor(

        Math.log(bytes) /
        Math.log(1024)

      );

    return (

      parseFloat(

        (
          bytes /
          Math.pow(
            1024,
            index
          )
        ).toFixed(2)

      ) +

      ' ' +

      units[index]
    );
  }
}
