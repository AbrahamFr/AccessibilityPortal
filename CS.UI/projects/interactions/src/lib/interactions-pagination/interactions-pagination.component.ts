import { Component, EventEmitter, Input, OnDestroy, Output } from '@angular/core';
import { InteractionsService } from '../interactions.service';

@Component({
  selector: 'interactions-pagination',
  templateUrl: './interactions-pagination.component.html',
  styleUrls: ['./interactions-pagination.component.scss']
})
export class InteractionsPaginationComponent {
  @Input()
  numberOfRecordsToShow: number;
  @Input()
  totalRecords: number;
  @Input()
  numberOfRecordsToFetchMore: number;

  @Output() seeMore = new EventEmitter<{
    recordsToReturn: number;
    numberOfRecordsToFetchMore: number;
  }>();
  

  constructor(
    private interactionsService: InteractionsService
  ) { }

  useCInvStyles = this.interactionsService.useCInvStyles;


  getMoreData = () => {
      let recordsToReturn = this.numberOfRecordsToShow + this.numberOfRecordsToFetchMore;
      if (recordsToReturn > this.totalRecords)
      {
        recordsToReturn = this.totalRecords
      }
      this.seeMore.emit({ recordsToReturn: recordsToReturn, numberOfRecordsToFetchMore: this.numberOfRecordsToFetchMore  })
  };
}
