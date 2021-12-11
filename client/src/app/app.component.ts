import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { TranslationService } from './_services/translation.service';
import {locale as arLang} from 'src/assets/i18n/ar'
import {locale as enLang} from 'src/assets/i18n/en';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dataing App';
  users: any;

  constructor(private http: HttpClient , private accountService : AccountService , private translationService: TranslationService) {
    // register translations
    this.translationService.loadTranslations(arLang,enLang);
  }
  ngOnInit() {

    this.setCurrentUser();
  }

  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    this.accountService.setCurrentUser(user);

  }

}
