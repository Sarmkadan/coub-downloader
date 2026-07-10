# BatchProcessingServiceTests

Unit test class for `BatchProcessingService`, verifying batch job creation, task management, state transitions, and lifecycle operations with mocked dependencies.

## API

### `BatchProcessingServiceTests`
Public test class containing async unit tests for batch processing functionality. Uses mocking frameworks to isolate service behavior from external dependencies.

### `Task CreateBatchJobAsync_ValidInputs_ReturnsNewBatchJob()`
Verifies that valid input parameters result in a new batch job being created with `Pending` state. No exceptions are thrown under correct conditions.

### `Task CreateBatchJobAsync_InvalidInputs_ThrowsArgumentException()`
Ensures that invalid input parameters (e.g., null or empty name, invalid priority) cause the method to throw `ArgumentException`.

### `Task AddTasksAsync_ValidInputs_AddsTasksToBatch()`
Confirms that valid task inputs are appended to an existing batch job in `Pending` state. The batch job state remains unchanged.

### `Task AddTasksAsync_BatchNotFound_ThrowsResourceNotFoundException()`
Validates that attempting to add tasks to a non-existent batch throws `ResourceNotFoundException`.

### `Task AddTasksAsync_BatchNotPending_ThrowsInvalidOperationException()`
Checks that adding tasks to a batch not in `Pending` state (e.g., `Completed`, `Failed`, `Cancelled`) throws `InvalidOperationException`.

### `Task StartBatchAsync_SuccessfulProcessing_UpdatesBatchStateToCompleted()`
Tests that starting a batch with all tasks succeeding transitions the batch state to `Completed` and marks all tasks as completed.

### `Task StartBatchAsync_SomeTasksFail_UpdatesBatchStateToFailed()`
Ensures that if any task fails during processing, the batch state transitions to `Failed` and the failure is recorded.

### `Task StartBatchAsync_BatchNotFound_ThrowsResourceNotFoundException()`
Confirms that attempting to start a non-existent batch throws `ResourceNotFoundException`.

### `Task StartBatchAsync_BatchCannotStart_ThrowsInvalidOperationException()`
Validates that starting a batch in an invalid state (e.g., already `Completed`, `Failed`, or `Cancelled`) throws `InvalidOperationException`.

### `Task StartBatchAsync_CancellationRequested_CancelsBatchAndTasks()`
Verifies that if cancellation is requested during batch processing, the batch and all its tasks are marked as `Cancelled`.

### `Task CancelBatchAsync_ValidBatch_UpdatesBatchAndTasksToCancelled()`
Ensures that a batch in `Pending` or `Processing` state can be cancelled, transitioning to `Cancelled` with all tasks also cancelled.

### `Task CancelBatchAsync_BatchNotFound_DoesNothing()`
Confirms that attempting to cancel a non-existent batch does not throw an exception and completes silently.

### `Task GetBatchStatusAsync_ValidBatchId_ReturnsBatchJob()`
Validates that retrieving the status of an existing batch returns the correct `BatchJob` object with current state and task details.

### `Task GetBatchStatusAsync_BatchNotFound_ThrowsResourceNotFoundException()`
Ensures that querying a non-existent batch throws `ResourceNotFoundException`.

### `Task GetAllBatchesAsync_ReturnsAllBatches()`
Tests that the method returns a complete list of all batch jobs, regardless of state.

### `Task GetActiveBatchesAsync_ReturnsOnlyActiveBatches()`
Confirms that only batches in `Pending` or `Processing` states are returned.

### `Task DeleteBatchAsync_ValidBatch_ReturnsTrueAndDeletes()`
Verifies that a batch in a terminal state (`Completed`, `Failed`, `Cancelled`) can be deleted, returning `true` and removing the batch from storage.

### `Task DeleteBatchAsync_BatchNotFound_ReturnsFalse()`
Ensures that attempting to delete a non-existent batch returns `false` without throwing.

### `Task DeleteBatchAsync_ProcessingBatch_ThrowsInvalidOperationException()`
Validates that deleting a batch in `Pending` or `Processing` state throws `InvalidOperationException`.

## Usage

### Example 1: Creating and processing a batch
