import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MisFavoritosComponent } from './mis-favoritos';

describe('MisFavoritos', () => {
  let component: MisFavoritosComponent;
  let fixture: ComponentFixture<MisFavoritosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MisFavoritosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MisFavoritosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
