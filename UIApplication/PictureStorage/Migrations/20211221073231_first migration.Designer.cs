﻿// <auto-generated />
using System;
using ImageStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ImageStorage.Migrations
{
    [DbContext(typeof(ImageLibraryContext))]
    [Migration("20211221073231_first migration")]
    partial class firstmigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("DbTableEntities.ImageDetails", b =>
                {
                    b.Property<int>("ImageDetailsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<int>("ImageInfoId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ImageDetailsId");

                    b.ToTable("ImagesDetails");
                });

            modelBuilder.Entity("DbTableEntities.ImageInformation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hash")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ImageDetailsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ImageDetailsId");

                    b.ToTable("ImagesInfo");
                });

            modelBuilder.Entity("DbTableEntities.RecognizedCategory", b =>
                {
                    b.Property<int>("ObjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Confidence")
                        .HasColumnType("REAL");

                    b.Property<int>("ImageInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImageInformationId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ObjectId");

                    b.HasIndex("ImageInformationId");

                    b.ToTable("RecognizedCategories");
                });

            modelBuilder.Entity("DbTableEntities.ImageInformation", b =>
                {
                    b.HasOne("DbTableEntities.ImageDetails", "ImageDetails")
                        .WithMany()
                        .HasForeignKey("ImageDetailsId");

                    b.Navigation("ImageDetails");
                });

            modelBuilder.Entity("DbTableEntities.RecognizedCategory", b =>
                {
                    b.HasOne("DbTableEntities.ImageInformation", null)
                        .WithMany("RecognizedCategories")
                        .HasForeignKey("ImageInformationId");
                });

            modelBuilder.Entity("DbTableEntities.ImageInformation", b =>
                {
                    b.Navigation("RecognizedCategories");
                });
#pragma warning restore 612, 618
        }
    }
}
