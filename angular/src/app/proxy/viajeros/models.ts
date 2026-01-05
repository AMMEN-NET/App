import type { EntityDto } from '@abp/ng.core';

export interface ViajeroDto extends EntityDto<string> {
  userName?: string;
  name?: string;
  surname?: string;
}
