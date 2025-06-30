import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResultPageComponent } from './result-page';


describe('ResultPageComponent', () => {
  let component: ResultPageComponent;
  let fixture: ComponentFixture<ResultPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResultPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ResultPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
