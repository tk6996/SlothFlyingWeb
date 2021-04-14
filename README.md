# Sloth Flying Website

เริ่มแรกในการเริ่มต้น Project ใช้คำสั่ง
```console
$ dotnet restore
```

หลังจากนั้นถ้ายังไม่ติดตั้ง Entity Framework Core tools ใช้คำสั่ง
```shell
$ dotnet tool install --global dotnet-ef
```

สามารถ Run ได้เลยโดยไม่ต้องทำตาม Step ด้านหลังถัดจากคำสั่งท้ายสุด

_________________________________________________________
ถ้าติดตั้งคำสั่งด้านบนเรียบร้อยแล้วสามารถใช้คำสั่งเพื่อสร้าง Migration File ได้
```shell
$ dotnet ef migrations add "NAME_OF_MIGRATION" #ชื่อ "NAME_OF_MIGRATION"
```

และใช้คำสั่งเพื่อ Update Database ตาม Migration
```shell
$ dotnet database update
```
กรณีที่ทำการสร้าง Model แล้ว Migration เรียบร้อยแล้วแต่ Update แล้วมีปัญหาสามารถลบ Database ลบ Migration และสร้างใหม่ได้
_________________________________________________________

เมื่อ Update เสร็จแล้วสามารถ Run เพื่อใช้งาน
```shell
$ dotnet watch run
```
p.s. กรณีที่ pull มาใหม่ควรลบ database ที่มีอยู่เดิมไปก่อนเพราะ schema อาจจะเปลี่ยน
