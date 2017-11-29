import { Tag } from './tag';
import { Step } from './step';
import { Link } from './link';
import { Attachment } from './attachment';
import { EmailTemplate } from './email-template';

export class Case {
  id: number;
  title: string;
  category: string;
  areaId: number;
  createdAt: Date;
  lastModifiedAt: Date;
  tags: Tag[] = [];
  steps: Step[] = [];
  links: Link[] = [];
  attachments: Attachment[] = [];
  emailTemplates: EmailTemplate[] = [];
}
