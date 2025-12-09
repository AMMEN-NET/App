import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MisFavoritos } from './mis-favoritos';

describe('MisFavoritos', () => {
  let component: MisFavoritos;
  let fixture: ComponentFixture<MisFavoritos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MisFavoritos]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MisFavoritos);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
