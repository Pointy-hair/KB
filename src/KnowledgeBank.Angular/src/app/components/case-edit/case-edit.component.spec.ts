import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CaseCreateComponent } from '../../components/case-create/case-create.component';

describe('CaseEditComponent', () => {
  let component: CaseCreateComponent;
  let fixture: ComponentFixture<CaseCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CaseCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CaseCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });
});
